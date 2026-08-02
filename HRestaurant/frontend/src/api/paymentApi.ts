import { getData, send } from "@/api/apiClient";
import type { PaymentSummary, ReceiptDto } from "@/api/contracts";
export const paymentKeys = { order: (id: string) => ["payments", "order", id] as const };
export const paymentApi = {
  summary: (orderId: string, signal?: AbortSignal) => getData<PaymentSummary>(`/payments/orders/${orderId}`, { signal }),
  create: (input: { orderId: string; paymentMethod: number; amount: number; transactionReference?: string }) => send<string>("post", "/payments", input),
  complete: (id: string, rowVersion: string) => send<PaymentSummary>("post", `/payments/${id}/complete`, { rowVersion }),
  split: (input: { orderId: string; orderRowVersion: string; payments: Array<{ paymentMethod: number; amount: number; transactionReference?: string }> }) => send<PaymentSummary>("post", "/payments/split", input),
  refund: (id: string, amount: number, reason: string, rowVersion: string) => send<PaymentSummary>("post", `/payments/${id}/refund`, { amount, reason, rowVersion }),
  receipt: (orderId: string, signal?: AbortSignal) => getData<ReceiptDto>(`/payments/orders/${orderId}/receipt`, { signal }),
};
