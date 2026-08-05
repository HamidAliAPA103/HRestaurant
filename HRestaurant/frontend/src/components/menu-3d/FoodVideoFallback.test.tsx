import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FoodVideoFallback } from "./FoodVideoFallback";

describe("FoodVideoFallback", () => {
  it("renders a playable 360 video when supplied", () => { render(<FoodVideoFallback name="Burger" videoUrl="/burger.webm" posterUrl="/burger.webp" />); expect(screen.getByLabelText("Burger üçün 360° video")).toBeInTheDocument(); });
  it("renders an accessible error when no video exists", () => { render(<FoodVideoFallback name="Burger" message="3D model göstərilə bilmədi" />); expect(screen.getByRole("alert")).toHaveTextContent("3D model göstərilə bilmədi"); });
});
