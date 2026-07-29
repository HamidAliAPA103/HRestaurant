import { apiClient } from "@/shared/api/client";
import type {
  ApiResponse,
  PagedResponse,
  PaginationParams,
} from "@/shared/types/api";
import { ViewType } from "@/shared/types/api";

export async function listResource<T>(
  endpoint: string,
  params: PaginationParams = {},
) {
  const { data } = await apiClient.get<PagedResponse<T>>(endpoint, {
    params: {
      type: params.type ?? ViewType.Active,
      pageNumber: params.pageNumber ?? 1,
      pageSize: params.pageSize ?? 100,
    },
  });

  return data;
}

export async function createResource<TInput, TData = string>(
  endpoint: string,
  input: TInput,
) {
  const { data } = await apiClient.post<ApiResponse<TData>>(
    endpoint,
    input,
  );
  return data;
}

export async function updateResource<TInput>(
  endpoint: string,
  id: string,
  input: TInput,
) {
  const { data } = await apiClient.patch<ApiResponse<unknown>>(
    endpoint,
    input,
    { params: { id } },
  );
  return data;
}

export async function patchResource<TInput>(
  endpoint: string,
  input: TInput,
) {
  const { data } = await apiClient.patch<ApiResponse<unknown>>(
    endpoint,
    input,
  );
  return data;
}
