import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Printer, RefreshCw, RotateCcw, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { orderApi } from "@/api/orderApi";
import { paymentApi, paymentKeys } from "@/api/paymentApi";
import type { ReceiptDto } from "@/api/contracts";
import { Button } from "@/shared/components/Button";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, getErrorMessage } from "@/shared/lib/utils";
import { OrderStatus } from "@/shared/types/domain";

interface Line { method: number; amount: number; reference: string }
const methods = ["Nağd", "Kart", "Bank köçürməsi", "Loyalty balı"];

function escapeHtml(value: unknown) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function printReceipt(data: ReceiptDto) {
  const receiptWindow = window.open("", "_blank", "width=720,height=900");
  if (!receiptWindow) {
    window.alert("Qəbz pəncərəsi brauzer tərəfindən bloklandı. Pop-up icazəsini aktiv edin.");
    return;
  }
  const itemRows = data.items.map((item) => `<tr><td>${escapeHtml(item.name)}</td><td>${item.quantity}</td><td>${escapeHtml(formatCurrency(item.unitPrice))}</td><td>${escapeHtml(formatCurrency(item.discount))}</td><td>${escapeHtml(formatCurrency(item.total))}</td></tr>`).join("");
  const paymentRows = data.payments.map((payment) => `<tr><td>${escapeHtml(methods[payment.method] ?? payment.method)}</td><td>${escapeHtml(formatCurrency(payment.amount))}</td><td>${escapeHtml(payment.transactionReference || "—")}</td></tr>`).join("");
  receiptWindow.document.write(`<!doctype html><html lang="az"><head><meta charset="utf-8"><title>Qəbz ${escapeHtml(data.orderNumber)}</title><style>@page{margin:12mm}*{box-sizing:border-box}body{max-width:720px;margin:0 auto;padding:24px;font:14px/1.45 Arial,sans-serif;color:#181411}h1,h2,p{margin:0}.head{text-align:center;border-bottom:2px solid #181411;padding-bottom:16px}.meta,.totals{margin-top:18px}.meta{display:grid;grid-template-columns:1fr 1fr;gap:6px}.muted{color:#6f655e}table{width:100%;border-collapse:collapse;margin-top:18px}th,td{padding:8px 6px;border-bottom:1px solid #ddd;text-align:right}th:first-child,td:first-child{text-align:left}.totals{margin-left:auto;width:min(320px,100%)}.line{display:flex;justify-content:space-between;padding:4px 0}.grand{border-top:2px solid #181411;margin-top:6px;padding-top:8px;font-size:18px;font-weight:700}.footer{text-align:center;margin-top:28px;padding-top:14px;border-top:1px dashed #999}@media print{body{padding:0}.no-print{display:none}}</style></head><body><header class="head"><h1>${escapeHtml(data.restaurantName)}</h1><p>${escapeHtml(data.branchName)}</p><p class="muted">${escapeHtml(data.address)}</p></header><section class="meta"><span>Sifariş</span><strong>${escapeHtml(data.orderNumber)}</strong><span>Masa</span><strong>${escapeHtml(data.tableNumber || "—")}</strong><span>Kassir</span><strong>${escapeHtml(data.cashierName)}</strong><span>Ödəniş vaxtı</span><strong>${escapeHtml(data.paidAt ? new Date(data.paidAt).toLocaleString("az-AZ") : "—")}</strong></section><table><thead><tr><th>Məhsul</th><th>Say</th><th>Qiymət</th><th>Endirim</th><th>Cəm</th></tr></thead><tbody>${itemRows}</tbody></table><section class="totals"><div class="line"><span>Ara cəm</span><strong>${escapeHtml(formatCurrency(data.subtotal))}</strong></div><div class="line"><span>Endirim</span><strong>${escapeHtml(formatCurrency(data.discount))}</strong></div><div class="line"><span>Vergi</span><strong>${escapeHtml(formatCurrency(data.tax))}</strong></div><div class="line grand"><span>Yekun</span><span>${escapeHtml(formatCurrency(data.total))}</span></div></section><h2>Ödənişlər</h2><table><thead><tr><th>Metod</th><th>Məbləğ</th><th>İstinad</th></tr></thead><tbody>${paymentRows}</tbody></table><p class="footer">Təşəkkür edirik!</p></body></html>`);
  receiptWindow.document.close();
  receiptWindow.focus();
  receiptWindow.print();
}

export function PaymentPage() {
  const [params, setParams] = useSearchParams(); const [orderId, setOrderId] = useState(params.get("orderId") ?? ""); const [lines, setLines] = useState<Line[]>([{ method: 0, amount: 0, reference: "" }]); const queryClient = useQueryClient();
  const orders = useQuery({ queryKey: ["orders", "payment-select"], queryFn: ({ signal }) => orderApi.list({ pageSize: 100, signal }) });
  const summary = useQuery({ queryKey: paymentKeys.order(orderId), queryFn: ({ signal }) => paymentApi.summary(orderId, signal), enabled: Boolean(orderId) });
  useEffect(() => { if (summary.data) setLines([{ method: 0, amount: summary.data.remainingAmount, reference: "" }]); }, [summary.data?.remainingAmount]);
  const pay = useMutation({ mutationFn: () => {
    if (!summary.data) throw new Error("Sifariş seçilməyib.");
    return paymentApi.split({ orderId, orderRowVersion: summary.data.orderRowVersion, payments: lines.map((line) => ({ paymentMethod: line.method, amount: line.amount, transactionReference: line.reference || undefined })) });
  }, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: paymentKeys.order(orderId) }); await queryClient.invalidateQueries({ queryKey: ["orders"] }); } });
  const refund = useMutation({ mutationFn: ({ id, max, rowVersion }: { id: string; max: number; rowVersion: string }) => { const raw = window.prompt(`Refund məbləği (maksimum ${max}):`, String(max)); const reason = window.prompt("Refund səbəbi:", "Müştəri tələbi"); if (raw === null || reason === null) throw new Error("Əməliyyat ləğv edildi."); return paymentApi.refund(id, Number(raw), reason, rowVersion); }, onSuccess: () => queryClient.invalidateQueries({ queryKey: paymentKeys.order(orderId) }) });
  const receipt = useMutation({ mutationFn: () => paymentApi.receipt(orderId), onSuccess: printReceipt });
  const selectable = (orders.data?.data ?? []).filter((x) => x.status !== OrderStatus.Cancelled);
  const entered = lines.reduce((sum, x) => sum + (Number.isFinite(x.amount) ? x.amount : 0), 0); const remaining = summary.data?.remainingAmount ?? 0;
  return <div className="page-enter space-y-6"><PageHeader eyebrow="Kassa" title="Ödənişlər" description="Split payment, refund və server qəbzi." actions={<Button variant="secondary" loading={summary.isFetching} disabled={!orderId} onClick={() => summary.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button>} /><div className="card p-5"><label className="text-sm font-semibold">Sifariş<select value={orderId} onChange={(e) => { setOrderId(e.target.value); setParams(e.target.value ? { orderId: e.target.value } : {}); }} className="mt-2 h-12 w-full rounded-xl border bg-white px-3"><option value="">Sifariş seçin</option>{selectable.map((order) => <option key={order.id} value={order.id}>{order.orderNumber} · {formatCurrency(order.totalAmount)} · {order.branchName}</option>)}</select></label></div>
    {!orderId ? <EmptyState title="Ödəniş üçün sifariş seçin" /> : summary.isLoading ? <LoadingState label="Ödəniş xülasəsi yüklənir" /> : summary.isError ? <ErrorState message={getErrorMessage(summary.error)} onRetry={() => summary.refetch()} /> : summary.data && <><section className="grid gap-4 sm:grid-cols-4">{[["Yekun", summary.data.totalAmount], ["Ödənib", summary.data.paidAmount], ["Refund", summary.data.refundedAmount], ["Qalıq", summary.data.remainingAmount]].map(([label,value]) => <div key={String(label)} className="card p-5"><p className="text-xs text-[#82776f]">{label}</p><p className="mt-2 text-xl font-bold">{formatCurrency(Number(value))}</p></div>)}</section>
      {!summary.data.isFullyPaid && <section className="card p-5"><div className="flex items-center justify-between"><div><h2 className="font-bold">Ödəniş hissələri</h2><p className="text-xs text-[#82776f]">Məbləğlər serverdə qalıqla yoxlanılır.</p></div><Button size="sm" variant="secondary" onClick={() => setLines((x) => [...x, { method: 1, amount: 0, reference: "" }])}><Plus className="h-4 w-4" />Hissə</Button></div><div className="mt-5 space-y-3">{lines.map((line,index) => <div key={index} className="grid gap-3 rounded-xl border p-3 sm:grid-cols-[1fr_1fr_1.4fr_auto]"><select value={line.method} onChange={(e) => setLines((all) => all.map((x,i) => i === index ? { ...x, method: Number(e.target.value) } : x))} className="h-10 rounded-xl border px-3">{methods.map((name,i) => <option key={name} value={i}>{name}</option>)}</select><input aria-label="Məbləğ" type="number" min={0.01} step="0.01" value={line.amount} onChange={(e) => setLines((all) => all.map((x,i) => i === index ? { ...x, amount: Number(e.target.value) } : x))} className="h-10 rounded-xl border px-3" /><input aria-label="Tranzaksiya istinadı" value={line.reference} onChange={(e) => setLines((all) => all.map((x,i) => i === index ? { ...x, reference: e.target.value } : x))} placeholder="Tranzaksiya istinadı" className="h-10 rounded-xl border px-3" /><button type="button" aria-label="Hissəni sil" disabled={lines.length === 1} className="rounded-lg p-2 text-red-600 disabled:opacity-40" onClick={() => setLines((all) => all.filter((_,i) => i !== index))}><Trash2 className="h-4 w-4" /></button></div>)}</div><div className="mt-5 flex flex-col items-end gap-3"><p className={`text-sm font-bold ${entered > remaining ? "text-red-600" : ""}`}>Daxil edilib: {formatCurrency(entered)} / {formatCurrency(remaining)}</p>{pay.isError && <p className="text-sm text-red-600">{getErrorMessage(pay.error)}</p>}<Button loading={pay.isPending} disabled={lines.some((x) => x.amount <= 0) || entered > remaining} onClick={() => pay.mutate()}>Ödənişi tamamla</Button></div></section>}
      <section className="card overflow-hidden"><div className="flex items-center justify-between border-b p-5"><h2 className="font-bold">Tranzaksiyalar</h2><Button size="sm" variant="secondary" disabled={!summary.data.payments.some((x) => x.paidAt)} loading={receipt.isPending} onClick={() => receipt.mutate()}><Printer className="h-4 w-4" />Qəbz</Button></div>{summary.data.payments.length === 0 ? <div className="p-5"><EmptyState title="Tranzaksiya yoxdur" /></div> : <div className="overflow-x-auto"><table className="data-table min-w-[700px]"><thead><tr><th>Metod</th><th>Məbləğ</th><th>Refund</th><th>Status</th><th>Kassir</th><th>Əməliyyat</th></tr></thead><tbody>{summary.data.payments.map((payment) => <tr key={payment.id}><td>{methods[payment.paymentMethod]}</td><td>{formatCurrency(payment.amount)}</td><td>{formatCurrency(payment.refundedAmount)}</td><td>{payment.paymentStatus}</td><td>{payment.createdByName}</td><td>{payment.refundableAmount > 0 && <button type="button" aria-label="Refund" className="rounded-lg p-2" onClick={() => refund.mutate({ id: payment.id, max: payment.refundableAmount, rowVersion: payment.rowVersion })}><RotateCcw className="h-4 w-4" /></button>}</td></tr>)}</tbody></table></div>}</section>
    </>}
  </div>;
}
