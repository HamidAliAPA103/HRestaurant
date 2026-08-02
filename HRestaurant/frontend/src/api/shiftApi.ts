import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams } from "@/api/contracts";
export interface ShiftDto { id: string; restaurantId: string; branchId: string; branchName: string; name: string; startTime: string; endTime: string; isActive: boolean; creatAt: string; updateAt: string | null }
export interface ShiftInput { restaurantId?: string; branchId?: string; name: string; startTime: string; endTime: string; isActive?: boolean }
export const shiftKeys = { all: ["shifts"] as const, assignments: ["shift-assignments"] as const };
export const shiftApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; branchId?: string } = {}) => getPage<ShiftDto>("/Shift", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<ShiftDto>(`/Shift/${id}`, { signal }),
  create: (input: ShiftInput & { restaurantId: string; branchId: string }) => send<string>("post", "/Shift", input),
  update: (id: string, input: ShiftInput & { isActive: boolean }) => send("put", `/Shift/${id}`, input),
  remove: (id: string) => send("delete", `/Shift/${id}`),
  assignments: (params: object = {}, signal?: AbortSignal) => getPage("/Shift/assignments", { params, signal }),
  assign: (input: object) => send<string>("post", "/Shift/assignments", input),
  unassign: (id: string) => send("delete", `/Shift/assignments/${id}`),
};
