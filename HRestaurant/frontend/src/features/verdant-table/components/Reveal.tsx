import type { ReactNode } from "react";
import { useScrollReveal } from "../hooks/useScrollReveal";

export function Reveal({ children, className = "", delay = 0 }: { children: ReactNode; className?: string; delay?: number }) {
  const { ref, visible } = useScrollReveal<HTMLDivElement>(delay);
  return <div ref={ref} className={`transition-[opacity,transform] duration-700 ease-out motion-reduce:transition-none ${visible ? "translate-y-0 opacity-100" : "translate-y-6 opacity-0"} ${className}`}>{children}</div>;
}
