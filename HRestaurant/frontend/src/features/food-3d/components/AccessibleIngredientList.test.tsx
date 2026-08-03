import { fireEvent, render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it } from "vitest";
import { getIngredientTransform } from "@/features/food-3d/lib/scene-utils";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicIngredient3D } from "@/types/public";
import { AccessibleIngredientList } from "./AccessibleIngredientList";

const ingredient = (
  id: string,
  name: string,
  positionX = 0,
): PublicIngredient3D => ({
  id,
  name,
  unit: "Gram",
  requiredQuantity: 25,
  model3DUrl: null,
  imageUrl: null,
  description: null,
  calories: null,
  protein: null,
  carbohydrates: null,
  fat: null,
  origin: null,
  allergenInformation: null,
  explodedPositionX: positionX,
  explodedPositionY: 0,
  explodedPositionZ: 0,
  explodedRotationX: 0,
  explodedRotationY: 90,
  explodedRotationZ: 0,
  displayOrder: 0,
  isVisibleIn3D: true,
  fallbackKind: "generic",
  usesProceduralFallback: true,
});

describe("AccessibleIngredientList", () => {
  beforeEach(() => useFoodViewerStore.getState().reset());

  it("supports arrow navigation and focuses the selected ingredient", () => {
    render(
      <AccessibleIngredientList
        ingredients={[
          ingredient("tomato", "Pomidor"),
          ingredient("cheese", "Pendir"),
        ]}
      />,
    );

    const tomato = screen.getByRole("button", { name: /Pomidor/i });
    const cheese = screen.getByRole("button", { name: /Pendir/i });
    tomato.focus();
    fireEvent.keyDown(tomato, { key: "ArrowDown" });
    expect(cheese).toHaveFocus();

    fireEvent.click(cheese);
    expect(useFoodViewerStore.getState().isExploded).toBe(true);
    expect(useFoodViewerStore.getState().selectedIngredientId).toBe("cheese");
  });

  it("uses saved exploded transforms and converts degrees to radians", () => {
    const transform = getIngredientTransform(ingredient("saved", "Saved", 3), 0, 1);

    expect(transform.position).toEqual([3, 0, 0]);
    expect(transform.rotation[1]).toBeCloseTo(Math.PI / 2);
  });
});
