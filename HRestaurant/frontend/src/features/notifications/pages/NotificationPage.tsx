import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCheck, Eye, RefreshCw } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { notificationApi, notificationKeys } from "@/api/notificationApi";
import type { NotificationDto } from "@/api/contracts";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatDate, getErrorMessage } from "@/shared/lib/utils";

const typeLabels = ["Az stok", "Stok bitib", "Son istifadə tarixi yaxınlaşır", "Müddəti bitib", "Yeni rezervasiya", "Rezervasiya statusu", "Sifariş hazırdır"];

export function NotificationPage() {
  const [selected, setSelected] = useState<NotificationDto | null>(null);
  const queryClient = useQueryClient();
  const query = useQuery({ queryKey: notificationKeys.all, queryFn: ({ signal }) => notificationApi.list({ pageSize: 100, signal }) });
  const action = useMutation({
    mutationFn: ({ type, id }: { type: "read" | "resolve" | "all"; id?: string }) => type === "all" ? notificationApi.markAllRead() : type === "resolve" ? notificationApi.resolve(id!) : notificationApi.markRead(id!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: notificationKeys.all });
      await queryClient.invalidateQueries({ queryKey: notificationKeys.unread });
    },
  });
  const openDetail = (item: NotificationDto) => {
    setSelected(item);
    if (!item.isRead) action.mutate({ type: "read", id: item.id });
  };
  const items = query.data?.data ?? [];

  return <div className="page-enter space-y-6">
    <PageHeader eyebrow="Xəbərdarlıqlar" title="Bildirişlər" description="Anbar, rezervasiya və sifariş hadisələri." actions={<div className="flex gap-2"><Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button><Button disabled={!items.some((item) => !item.isRead)} onClick={() => action.mutate({ type: "all" })}><CheckCheck className="h-4 w-4" />Hamısını oxu</Button></div>} />
    {query.isLoading ? <LoadingState label="Bildirişlər yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : items.length === 0 ? <EmptyState title="Bildiriş yoxdur" /> : <div className="space-y-3">{items.map((item) => <article key={item.id} className={`card flex flex-col gap-4 p-5 sm:flex-row sm:items-center ${item.isRead ? "opacity-75" : "border-[#ef9a87]"}`}>
      <div className="min-w-0 flex-1"><div className="flex flex-wrap gap-2"><h2 className="font-bold">{item.title}</h2><Badge tone="info">{typeLabels[item.type] ?? "Sistem"}</Badge>{!item.isRead && <Badge tone="warning">Yeni</Badge>}{item.isResolved && <Badge tone="success">Həll edilib</Badge>}</div><p className="mt-2 text-sm text-[#756c64]">{item.message}</p><p className="mt-2 text-xs text-[#91877f]">{item.ingredientName ? `${item.ingredientName} · ` : ""}{formatDate(item.creatAt, true)}</p></div>
      <div className="flex gap-2"><Button size="sm" variant="secondary" onClick={() => openDetail(item)}><Eye className="h-4 w-4" />Detallar</Button>{!item.isRead && <Button size="sm" variant="secondary" onClick={() => action.mutate({ type: "read", id: item.id })}>Oxundu</Button>}{item.type <= 3 && !item.isResolved && <Button size="sm" onClick={() => action.mutate({ type: "resolve", id: item.id })}>Həll et</Button>}</div>
    </article>)}</div>}
    {action.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(action.error)}</p>}
    <Modal open={Boolean(selected)} onClose={() => setSelected(null)} title={selected?.title ?? "Bildiriş"} description={selected ? formatDate(selected.creatAt, true) : ""}>{selected && <div className="space-y-4"><Badge tone="info">{typeLabels[selected.type] ?? "Sistem"}</Badge><p className="text-sm leading-6 text-[#655d57]">{selected.message}</p>{selected.targetUrl && <div className="flex justify-end"><Link to={selected.targetUrl} onClick={() => setSelected(null)}><Button>Əlaqəli qeydə bax</Button></Link></div>}</div>}</Modal>
  </div>;
}
