import { useGLTF } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { useEffect, useMemo, useRef } from "react";
import { Group, MathUtils, Mesh, Object3D } from "three";
import { clone } from "three/examples/jsm/utils/SkeletonUtils.js";
import { caesarSaladIngredients } from "@/data/caesar-salad.ingredients";
import { IngredientLabel } from "./IngredientLabel";

const requiredMeshes: string[] = caesarSaladIngredients.map((ingredient) => ingredient.meshName);

export function FoodModel({ progress, compact, onMissingMeshes }: { progress: React.MutableRefObject<number>; compact: boolean; onMissingMeshes: (names: string[]) => void }) {
  const { scene } = useGLTF("/models/caesar-salad.glb");
  const root = useRef<Group>(null);
  const originals = useRef(new Map<string, { object: Object3D; position: [number, number, number]; rotation: [number, number, number]; scale: [number, number, number] }>());
  const model = useMemo(() => clone(scene), [scene]);

  useEffect(() => {
    const found = new Set<string>();
    model.traverse((node) => {
      if (!(node instanceof Mesh)) return;
      node.castShadow = true; node.receiveShadow = true;
      if (!requiredMeshes.includes(node.name)) return;
      found.add(node.name);
      originals.current.set(node.name, { object: node, position: [node.position.x, node.position.y, node.position.z], rotation: [node.rotation.x, node.rotation.y, node.rotation.z], scale: [node.scale.x, node.scale.y, node.scale.z] });
    });
    const missing = requiredMeshes.filter((name) => !found.has(name));
    if (missing.length && import.meta.env.DEV) console.warn("[CaesarSalad] GLB model is missing required ingredient meshes:", missing);
    onMissingMeshes(missing);
  }, [model, onMissingMeshes]);

  useFrame((_, delta) => {
    const explode = MathUtils.smoothstep(progress.current, 0.3, 0.7) * (compact ? 0.58 : 1);
    root.current?.rotation.set(0, Math.sin(progress.current * Math.PI) * 0.13 + delta * 0, 0);
    caesarSaladIngredients.forEach((ingredient) => {
      const original = originals.current.get(ingredient.meshName); if (!original) return;
      const [x, y, z] = ingredient.explodedPosition;
      original.object.position.lerp({ x: original.position[0] + x * explode, y: original.position[1] + y * explode, z: original.position[2] + z * explode }, Math.min(1, delta * 8));
    });
  });
  return <group ref={root} scale={compact ? 1.05 : 1.25}><primitive object={model} />{caesarSaladIngredients.map((ingredient) => <IngredientLabel key={ingredient.meshName} ingredient={ingredient} progress={progress} compact={compact} />)}</group>;
}
