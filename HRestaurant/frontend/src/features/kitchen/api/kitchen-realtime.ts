import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuthStore } from "@/features/auth/store/auth-store";
import type { KitchenOrder } from "@/shared/types/domain";

export interface KitchenOrderEvent {
  eventName: string;
  order: KitchenOrder;
  occurredAtUtc: string;
  audioCue?: string | null;
}

export function createKitchenConnection(onEvent: (event: KitchenOrderEvent) => void) {
  const configuredApi = import.meta.env.VITE_API_BASE_URL || "/api";
  const configuredHub = import.meta.env.VITE_SIGNALR_BASE_URL?.replace(/\/$/, "");
  const hubUrl = configuredHub
    ? `${configuredHub}/hubs/kitchen`
    : configuredApi.endsWith("/api")
      ? `${configuredApi.slice(0, -4)}/hubs/kitchen`
      : `${configuredApi.replace(/\/$/, "")}/hubs/kitchen`;
  const connection = new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => useAuthStore.getState().accessToken ?? "",
    })
    .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
    .configureLogging(LogLevel.Warning)
    .build();

  ["OrderCreated", "OrderUpdated", "OrderStatusChanged", "OrderCancelled", "OrderReady"]
    .forEach((eventName) => connection.on(eventName, onEvent));
  return connection;
}
