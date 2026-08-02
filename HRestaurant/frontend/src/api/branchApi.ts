import { getData, getPage, send } from "@/api/apiClient";
import type { BranchDto, BranchInput, ListParams, WorkingHour } from "@/api/contracts";
export const branchKeys = { all: ["branches"] as const, detail: (id: string) => ["branches", id] as const };
export const branchApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string } = {}) => getPage<BranchDto>("/Branch", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<BranchDto>(`/Branch/${id}`, { signal }),
  create: (input: BranchInput & { restaurantId: string }) => send<string>("post", "/Branch", input),
  update: (id: string, input: BranchInput) => send("put", `/Branch/${id}`, input),
  remove: (id: string) => send("delete", `/Branch/${id}`),
  setActive: (id: string, active: boolean) => send("patch", `/Branch/${id}/${active ? "activate" : "deactivate"}`),
  assignManager: (id: string, managerId: string) => send("put", `/Branch/${id}/manager`, { managerId }),
  removeManager: (id: string) => send("delete", `/Branch/${id}/manager`),
  updateWorkingHours: (id: string, workingHours: WorkingHour[]) => send("put", `/Branch/${id}/working-hours`, { workingHours }),
};
