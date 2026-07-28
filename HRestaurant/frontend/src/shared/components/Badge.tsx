import type { ReactNode } from "react";
import { cn } from "@/shared/lib/utils";

export function Badge({
  children,
  tone = "neutral",
  dot = false,
}: {
  children: ReactNode;
  tone?: "success" | "warning" | "danger" | "info" | "neutral";
  dot?: boolean;
}) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold",
        tone === "success" && "bg-[#e4f5e9] text-[#247441]",
        tone === "warning" && "bg-[#fff3d7] text-[#966614]",
        tone === "danger" && "bg-[#ffe8e4] text-[#b74731]",
        tone === "info" && "bg-[#e7f0ff] text-[#3769a8]",
        tone === "neutral" && "bg-[#eeeae4] text-[#645c55]",
      )}
    >
      {dot && <span className="h-1.5 w-1.5 rounded-full bg-current" />}
      {children}
    </span>
  );
}
