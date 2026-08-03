import { Line } from "@react-three/drei";
import { memo } from "react";
import type { VectorTuple } from "@/features/food-3d/lib/scene-utils";

interface IngredientConnectorLineProps {
  to: VectorTuple;
  highlighted: boolean;
}

export const IngredientConnectorLine = memo(function IngredientConnectorLine({
  to,
  highlighted,
}: IngredientConnectorLineProps) {
  return (
    <Line
      points={[[0, 0.75, 0], to]}
      color={highlighted ? "#f15a3b" : "#a78e7b"}
      lineWidth={highlighted ? 1.8 : 0.7}
      transparent
      opacity={highlighted ? 0.9 : 0.34}
      dashed={!highlighted}
      dashSize={0.08}
      gapSize={0.08}
    />
  );
});
