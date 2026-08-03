import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams, User } from "@/api/contracts";

const employeeWriteTimeout = 60_000;

export interface EmployeeInput {
  restaurantId: string; branchId: string; email: string; name: string;
  phone: string; role: string; salary: number; hireDate: string;
  avatarUrl?: string; emergencyContact: string; password: string;
}
export const employeeKeys = { all: ["employees"] as const, detail: (id: string) => ["employees", id] as const };
export const employeeApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; branchId?: string; role?: string; isActive?: boolean } = {}) => getPage<User>("/User", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<User>(`/User/${id}`, { signal }),
  create: (input: EmployeeInput) => send<string>("post", "/User", input, { timeout: employeeWriteTimeout }),
  update: (id: string, input: Partial<Omit<EmployeeInput, "password" | "restaurantId" | "branchId">>) => send("put", `/User/${id}`, input, { timeout: employeeWriteTimeout }),
  remove: (id: string) => send("delete", `/User/${id}`),
  setActive: (id: string, active: boolean) => send("patch", `/User/${id}/${active ? "activate" : "deactivate"}`),
  assignBranch: (id: string, branchId: string) => send("put", `/User/${id}/branch`, { branchId }),
  assignRole: (id: string, role: string) => send("put", `/User/${id}/role`, { role }),
};
