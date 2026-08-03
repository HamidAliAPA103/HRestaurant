import { RoundedBox, useGLTF } from "@react-three/drei";
import gsap from "gsap";
import { memo, useEffect, useLayoutEffect, useMemo, useRef } from "react";
import {
  DoubleSide,
  InstancedMesh,
  Material,
  Mesh,
  Object3D,
  type Group,
} from "three";
import { clone as cloneSkeleton } from "three/examples/jsm/utils/SkeletonUtils.js";
import {
  cloneMaterialWithTextures,
  disposeMaterialAndTextures,
} from "@/features/food-3d/lib/model-resources";
import type { PublicIngredient3D } from "@/types/public";

interface IngredientModel3DProps {
  ingredient: PublicIngredient3D;
  dimmed: boolean;
}

function LoadedIngredientModel({ url }: { url: string }) {
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
      clonedScene.traverse((object) => {
        if (!(object instanceof Mesh)) return;
        const materials = Array.isArray(object.material)
          ? object.material
          : [object.material];
        materials.forEach(disposeMaterialAndTextures);
      });
    },
    [clonedScene],
  );

  return <primitive object={clonedScene} dispose={null} />;
}

const leafTransforms = [
  [-0.18, 0.02, -0.06, -0.3],
  [0.02, 0.1, 0.1, 0.25],
  [0.2, 0.03, -0.1, 0.55],
  [-0.08, 0.19, 0.03, -0.7],
  [0.12, 0.24, 0.07, 0.9],
  [-0.24, 0.14, 0.11, -1.1],
] as const;

function HerbFallback() {
  const meshRef = useRef<InstancedMesh>(null);
  const transform = useMemo(() => new Object3D(), []);

  useLayoutEffect(() => {
    if (!meshRef.current) return;
    leafTransforms.forEach(([x, y, z, rotation], index) => {
      transform.position.set(x, y, z);
      transform.rotation.set(-Math.PI / 2, 0, rotation);
      transform.scale.set(0.35, 0.17, 0.35);
      transform.updateMatrix();
      meshRef.current?.setMatrixAt(index, transform.matrix);
    });
    meshRef.current.instanceMatrix.needsUpdate = true;
  }, [transform]);

  return (
    <instancedMesh
      ref={meshRef}
      args={[undefined, undefined, leafTransforms.length]}
      castShadow
      frustumCulled={false}
    >
      <planeGeometry args={[1, 1]} />
      <meshStandardMaterial color="#3f8b42" roughness={0.82} side={DoubleSide} />
    </instancedMesh>
  );
}

function ProceduralIngredient({ ingredient }: { ingredient: PublicIngredient3D }) {
  switch (ingredient.fallbackKind) {
    case "tomato":
      return (
        <mesh castShadow>
          <sphereGeometry args={[0.34, 32, 20]} />
          <meshStandardMaterial color="#d74632" roughness={0.58} />
        </mesh>
      );
    case "cucumber":
      return (
        <mesh castShadow rotation={[0, 0, Math.PI / 2]}>
          <cylinderGeometry args={[0.22, 0.22, 0.78, 28]} />
          <meshStandardMaterial color="#4f8f48" roughness={0.7} />
        </mesh>
      );
    case "cheese":
      return (
        <RoundedBox args={[0.72, 0.3, 0.55]} radius={0.08} smoothness={4} castShadow>
          <meshStandardMaterial color="#f5c84c" roughness={0.62} />
        </RoundedBox>
      );
    case "sauce":
      return (
        <group>
          <mesh castShadow>
            <cylinderGeometry args={[0.42, 0.34, 0.16, 36]} />
            <meshPhysicalMaterial
              color="#a92720"
              roughness={0.28}
              transmission={0.15}
              transparent
              opacity={0.72}
            />
          </mesh>
          <mesh position={[0, 0.09, 0]} rotation={[-Math.PI / 2, 0, 0]}>
            <circleGeometry args={[0.35, 36]} />
            <meshStandardMaterial color="#d54a35" transparent opacity={0.86} />
          </mesh>
        </group>
      );
    case "herb":
      return <HerbFallback />;
    default:
      return (
        <RoundedBox args={[0.58, 0.48, 0.58]} radius={0.16} smoothness={5} castShadow>
          <meshStandardMaterial color="#cf895b" roughness={0.72} />
        </RoundedBox>
      );
  }
}

export const IngredientModel3D = memo(function IngredientModel3D({
  ingredient,
  dimmed,
}: IngredientModel3DProps) {
  const groupRef = useRef<Group>(null);

  useEffect(() => {
    const group = groupRef.current;
    if (!group) return;

    const materials: Material[] = [];
    group.traverse((object) => {
      if (!(object instanceof Mesh) && !(object instanceof InstancedMesh)) return;
      const objectMaterials = Array.isArray(object.material)
        ? object.material
        : [object.material];
      objectMaterials.forEach((material) => {
        const storedOpacity = material.userData.food3dBaseOpacity;
        const baseOpacity =
          typeof storedOpacity === "number" ? storedOpacity : material.opacity;
        material.userData.food3dBaseOpacity = baseOpacity;
        material.transparent = dimmed || baseOpacity < 1;
        materials.push(material);
        gsap.to(material, {
          opacity: dimmed ? baseOpacity * 0.22 : baseOpacity,
          duration: 0.2,
          overwrite: true,
        });
      });
    });

    return () => {
      materials.forEach((material) => gsap.killTweensOf(material));
    };
  }, [dimmed]);

  return (
    <group ref={groupRef} scale={0.9}>
      {ingredient.model3DUrl ? (
        <LoadedIngredientModel url={ingredient.model3DUrl} />
      ) : (
        <ProceduralIngredient ingredient={ingredient} />
      )}
    </group>
  );
});
