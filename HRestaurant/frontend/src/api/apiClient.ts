import type { AxiosRequestConfig } from "axios";
import { apiClient } from "@/shared/api/client";
import type { ApiResponse, PagedResponse } from "@/shared/types/api";

export { apiClient };

export async function getData<T>(url: string, config?: AxiosRequestConfig) {
  const { data } = await apiClient.get<ApiResponse<T>>(url, config);
  if (!data.success || data.data === null) throw new Error(data.message);
  return data.data;
}

export async function getPage<T>(url: string, config?: AxiosRequestConfig) {
  const { data } = await apiClient.get<PagedResponse<T>>(url, config);
  if (!data.success) throw new Error(data.message);
  return data;
}

export async function send<T>(
  method: "post" | "put" | "patch" | "delete",
  url: string,
  body?: unknown,
  config?: AxiosRequestConfig,
) {
  const { data } = await apiClient.request<ApiResponse<T>>({
    ...config,
    method,
    url,
    data: body,
  });
  if (!data.success && data.statusCode !== 204) throw new Error(data.message);
  return data;
}
