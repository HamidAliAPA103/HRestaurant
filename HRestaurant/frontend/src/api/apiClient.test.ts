import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/shared/api/client", () => ({
  apiClient: {
    get: vi.fn(),
    request: vi.fn(),
  },
}));

import { apiClient, send } from "@/api/apiClient";

const requestMock = vi.mocked(apiClient.request);

describe("send", () => {
  beforeEach(() => requestMock.mockReset());

  it("treats a 204 delete response as a successful API response", async () => {
    requestMock.mockResolvedValue({ status: 204, data: undefined } as never);

    await expect(send("delete", "/categories/test-id")).resolves.toEqual({
      success: true,
      message: "Operation completed successfully.",
      data: null,
      errors: [],
      statusCode: 204,
    });
  });
});
