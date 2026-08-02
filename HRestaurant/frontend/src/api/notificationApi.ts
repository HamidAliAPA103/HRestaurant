import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams, NotificationDto } from "@/api/contracts";
export const notificationKeys = { all: ["notifications"] as const, unread: ["notifications", "unread"] as const };
export const notificationApi = {
  list: ({ signal, ...params }: ListParams & { branchId?: string; isRead?: boolean; isResolved?: boolean } = {}) => getPage<NotificationDto>("/notifications", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<NotificationDto>(`/notifications/${id}`, { signal }),
  unreadCount: (branchId?: string, signal?: AbortSignal) => getData<number>("/notifications/unread/count", { params: { branchId }, signal }),
  markRead: (id: string) => send("patch", `/notifications/${id}/read`),
  markAllRead: (branchId?: string) => send("patch", "/notifications/read-all", undefined, { params: { branchId } }),
  resolve: (id: string) => send("patch", `/notifications/${id}/resolve`),
};
