import { useGLTF } from "@react-three/drei";
import { memo, useEffect, useMemo } from "react";
import { MathUtils, Mesh } from "three";
import { clone as cloneSkeleton } from "three/examples/jsm/utils/SkeletonUtils.js";
import {
  cloneMaterialWithTextures,
  disposeMaterialAndTextures,
} from "@/features/food-3d/lib/model-resources";
import type { PublicFood3D } from "@/types/public";

interface FoodModelProps {
  food: PublicFood3D;
  forceProcedural?: boolean;
}

function disposeClonedMaterials(root: ReturnType<typeof cloneSkeleton>) {
  root.traverse((object) => {
    if (!(object instanceof Mesh)) return;
    const materials = Array.isArray(object.material)
      ? object.material
      : [object.material];
    materials.forEach(disposeMaterialAndTextures);
  });
}

function LoadedFoodModel({ url }: { url: string }) {
  const { scene } = useGLTF(url);
  const clonedScene = useMemo(() => {
    const clone = cloneSkeleton(scene);
    clone.traverse((object) => {
      if (!(object instanceof Mesh)) return;
      object.castShadow = true;
      object.receiveShadow = true;
      object.material = Array.isArray(object.material)
        ? object.material.map(cloneMaterialWithTextures)
        : cloneMaterialWithTextures(object.material);
    });
    return clone;
  }, [scene]);

  useEffect(
    () => () => {
      clonedScene.removeFromParent();
      disposeClonedMaterials(clonedScene);
    },
    [clonedScene],
  );

  return <primitive object={clonedScene} dispose={null} />;
}

export const ProceduralFoodModel = memo(function ProceduralFoodModel() {
  return (
    <group position={[0, 0.43, 0]}>
      <mesh castShadow receiveShadow position={[0, 0.18, 0]} scale={[1.28, 0.38, 1.28]}>
        <sphereGeometry args={[0.9, 48, 24]} />
        <meshStandardMaterial color="#e0a552" roughness={0.72} />
      </mesh>
      <mesh castShadow position={[0, 0.54, 0]} scale={[1.13, 0.16, 1.13]}>
        <sphereGeometry args={[0.82, 40, 20]} />
        <meshStandardMaterial color="#5c8c45" roughness={0.82} />
      </mesh>
      <mesh castShadow position={[0, 0.7, 0]} scale={[1.16, 0.12, 1.16]}>
        <cylinderGeometry args={[0.7, 0.75, 0.3, 48]} />
        <meshStandardMaterial color="#b83b2d" roughness={0.65} />
      </mesh>
      <mesh castShadow position={[0, 0.95, 0]} scale={[1.28, 0.46, 1.28]}>
        <sphereGeometry args={[0.9, 48, 24, 0, Math.PI * 2, 0, Math.PI / 2]} />
        <meshStandardMaterial color="#e7af5e" roughness={0.68} />
      </mesh>
    </group>
  );
});

export const FoodModel = memo(function FoodModel({
  food,
  forceProcedural = false,
}: FoodModelProps) {
  const canLoadModel =
    !forceProcedural && food.is3DEnabled && Boolean(food.model3DUrl);

  return (
    <group
      scale={food.modelScale || 1}
      rotation={[
        MathUtils.degToRad(food.modelRotationX),
        MathUtils.degToRad(food.modelRotationY),
        MathUtils.degToRad(food.modelRotationZ),
      ]}
    >
      {canLoadModel && food.model3DUrl ? (
        <LoadedFoodModel url={food.model3DUrl} />
      ) : (
        <ProceduralFoodModel />
      )}
    </group>
  );
});
