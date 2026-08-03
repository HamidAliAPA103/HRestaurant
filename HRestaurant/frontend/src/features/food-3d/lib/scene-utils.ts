import type { PublicIngredient3D } from "@/types/public";

export type VectorTuple = [number, number, number];

export interface IngredientTransform {
  position: VectorTuple;
  rotation: VectorTuple;
}

const degreesToRadians = (degrees: number) => (degrees * Math.PI) / 180;

export function getIngredientTransform(
  ingredient: PublicIngredient3D,
  index: number,
  total: number,
): IngredientTransform {
  const hasConfiguredPosition =
    Math.abs(ingredient.explodedPositionX) +
      Math.abs(ingredient.explodedPositionY) +
      Math.abs(ingredient.explodedPositionZ) >
    0.001;

  const angle = (index / Math.max(total, 1)) * Math.PI * 2 - Math.PI / 2;
  const radius = 2.2 + (index % 2) * 0.35;
  const position: VectorTuple = hasConfiguredPosition
    ? [
        ingredient.explodedPositionX,
        ingredient.explodedPositionY,
        ingredient.explodedPositionZ,
      ]
    : [
        Math.cos(angle) * radius,
        0.85 + (index % 3) * 0.25,
        Math.sin(angle) * radius,
      ];

  return {
    position,
    rotation: [
      degreesToRadians(ingredient.explodedRotationX),
      degreesToRadians(ingredient.explodedRotationY),
      degreesToRadians(ingredient.explodedRotationZ),
    ],
  };
}

export function supportsWebGL() {
  if (typeof document === "undefined") return false;

  try {
    const canvas = document.createElement("canvas");
    const context =
      canvas.getContext("webgl2") ?? canvas.getContext("webgl");
    const loseContext = context?.getExtension("WEBGL_lose_context");
    loseContext?.loseContext();
    return context !== null;
  } catch {
    return false;
  }
}
