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
  const response = await apiClient.request<ApiResponse<T>>({
    ...config,
    method,
    url,
    data: body,
  });
  if (response.status === 204) {
    return {
      success: true,
      message: "Operation completed successfully.",
      data: null,
      errors: [],
      statusCode: 204,
    } satisfies ApiResponse<T>;
  }
  const { data } = response;
  if (!data.success && data.statusCode !== 204) throw new Error(data.message);
  return data;
}
