import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { FoodViewerControls } from "./FoodViewerControls";

describe("FoodViewerControls", () => {
  it("disables motion-only auto rotation for reduced-motion users", () => { render(<FoodViewerControls autoRotate={false} reducedMotion onAutoRotate={vi.fn()} onReset={vi.fn()} onFullscreen={vi.fn()} />); expect(screen.getByRole("button", { name: "Avtomatik fırlanmanı dəyiş" })).toBeDisabled(); });
});
