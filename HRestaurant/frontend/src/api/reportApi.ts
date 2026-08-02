import { getData, getPage } from "@/api/apiClient";
import type { DashboardSummary, NamedValue, TimeSeriesPoint } from "@/api/contracts";
export interface ReportParams { from?: string; to?: string; branchId?: string; pageNumber?: number; pageSize?: number; signal?: AbortSignal }
function config({ signal, ...params }: ReportParams) { return { signal, params }; }
export const reportKeys = { all: ["reports"] as const, dashboard: (params: object) => ["reports", "dashboard", params] as const };
export const reportApi = {
  dashboard: (params: ReportParams = {}) => getData<DashboardSummary>("/reports/dashboard", config(params)),
  sales: (params: ReportParams & { period?: string } = {}) => getData<TimeSeriesPoint[]>("/reports/sales", config(params)),
  categories: (params: ReportParams = {}) => getData<NamedValue[]>("/reports/categories", config(params)),
  paymentMethods: (params: ReportParams = {}) => getData<NamedValue[]>("/reports/payment-methods", config(params)),
  branches: (params: ReportParams = {}) => getData<NamedValue[]>("/reports/branches", config(params)),
  menuItems: (params: ReportParams & { least?: boolean } = {}) => getPage<NamedValue>("/reports/menu-items", config(params)),
  lowStock: (params: ReportParams = {}) => getData<NamedValue[]>("/reports/low-stock", config(params)),
};
