import gsap from "gsap";
import { useReducedMotion } from "motion/react";
import { memo, useLayoutEffect, useMemo, useRef } from "react";
import type { ThreeEvent } from "@react-three/fiber";
import type { Group } from "three";
import { getIngredientTransform } from "@/features/food-3d/lib/scene-utils";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicIngredient3D } from "@/types/public";
import { IngredientConnectorLine } from "./IngredientConnectorLine";
import { IngredientLabel3D } from "./IngredientLabel3D";
import { IngredientModel3D } from "./IngredientModel3D";

interface IngredientExplodedViewProps {
  ingredients: PublicIngredient3D[];
}

interface ExplodedIngredientProps {
  ingredient: PublicIngredient3D;
  index: number;
  total: number;
  isExploded: boolean;
  isHovered: boolean;
  isSelected: boolean;
  dimmed: boolean;
}

const ExplodedIngredient = memo(function ExplodedIngredient({
  ingredient,
  index,
  total,
  isExploded,
  isHovered,
  isSelected,
  dimmed,
}: ExplodedIngredientProps) {
  const groupRef = useRef<Group>(null);
  const reducedMotion = useReducedMotion();
  const setHoveredIngredient = useFoodViewerStore(
    (state) => state.setHoveredIngredient,
  );
  const setSelectedIngredient = useFoodViewerStore(
    (state) => state.setSelectedIngredient,
  );
  const transform = useMemo(
    () => getIngredientTransform(ingredient, index, total),
    [ingredient, index, total],
  );

  useLayoutEffect(() => {
    const group = groupRef.current;
    if (!group) return;
    const position = isExploded ? transform.position : [0, 0.72, 0];
    const rotation = isExploded ? transform.rotation : [0, 0, 0];
    const scale = isExploded ? 1 : 0.001;

    if (reducedMotion) {
      group.position.set(position[0], position[1], position[2]);
      group.rotation.set(rotation[0], rotation[1], rotation[2]);
      group.scale.setScalar(scale);
      return;
    }

    const timeline = gsap.timeline({ defaults: { overwrite: true } });
    const delay = isExploded ? index * 0.045 : 0;
    timeline
      .to(
        group.position,
        {
          x: position[0],
          y: position[1],
          z: position[2],
          duration: 0.72,
          delay,
          ease: "power3.inOut",
        },
        0,
      )
      .to(
        group.rotation,
        {
          x: rotation[0],
          y: rotation[1],
          z: rotation[2],
          duration: 0.72,
          delay,
          ease: "power3.inOut",
        },
        0,
      )
      .to(
        group.scale,
        {
          x: scale,
          y: scale,
          z: scale,
          duration: isExploded ? 0.42 : 0.3,
          delay,
          ease: "back.out(1.5)",
        },
        0,
      );

    return () => {
      timeline.kill();
    };
  }, [index, isExploded, reducedMotion, transform]);

  const stopAndSelect = (event: ThreeEvent<MouseEvent>) => {
    event.stopPropagation();
    setSelectedIngredient(ingredient.id);
  };

  return (
    <group
      ref={groupRef}
      scale={0.001}
      onPointerOver={(event) => {
        event.stopPropagation();
        setHoveredIngredient(ingredient.id);
        document.body.style.cursor = "pointer";
      }}
      onPointerOut={(event) => {
        event.stopPropagation();
        setHoveredIngredient(null);
        document.body.style.cursor = "default";
      }}
      onClick={stopAndSelect}
    >
      <IngredientModel3D ingredient={ingredient} dimmed={dimmed} />
      <IngredientLabel3D
        ingredient={ingredient}
        visible={isHovered || isSelected}
      />
    </group>
  );
});

export const IngredientExplodedView = memo(function IngredientExplodedView({
  ingredients,
}: IngredientExplodedViewProps) {
  const isExploded = useFoodViewerStore((state) => state.isExploded);
  const hoveredIngredientId = useFoodViewerStore(
    (state) => state.hoveredIngredientId,
  );
  const selectedIngredientId = useFoodViewerStore(
    (state) => state.selectedIngredientId,
  );
  const visibleIngredients = useMemo(
    () => ingredients.filter((ingredient) => ingredient.isVisibleIn3D),
    [ingredients],
  );

  return (
    <group>
      {isExploded &&
        visibleIngredients.map((ingredient, index) => {
          const transform = getIngredientTransform(
            ingredient,
            index,
            visibleIngredients.length,
          );
          return (
            <IngredientConnectorLine
              key={`line-${ingredient.id}`}
              to={transform.position}
              highlighted={
                hoveredIngredientId === ingredient.id ||
                selectedIngredientId === ingredient.id
              }
            />
          );
        })}
      {visibleIngredients.map((ingredient, index) => (
        <ExplodedIngredient
          key={ingredient.id}
          ingredient={ingredient}
          index={index}
          total={visibleIngredients.length}
          isExploded={isExploded}
          isHovered={hoveredIngredientId === ingredient.id}
          isSelected={selectedIngredientId === ingredient.id}
          dimmed={
            hoveredIngredientId !== null && hoveredIngredientId !== ingredient.id
          }
        />
      ))}
    </group>
  );
});
