import { Center, Html, useGLTF } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { useEffect, useMemo } from "react";
import { MathUtils, Object3D } from "three";
import { clone } from "three/examples/jsm/utils/SkeletonUtils.js";
import { ingredientPartNames } from "./types";

export function FoodModel({ url, scale, rotation, exploded, reducedMotion }: { url: string; scale: number; rotation: [number, number, number]; exploded: boolean; reducedMotion: boolean }) {
  const { scene } = useGLTF(url, true, true);
  const model = useMemo(() => clone(scene), [scene]);
  const parts = useMemo(() => Object.keys(ingredientPartNames).map((name) => model.getObjectByName(name)).filter((part): part is Object3D => Boolean(part)).map((part, index) => ({ part, y: part.position.y, offset: (index - 3) * 0.42 })), [model]);
  useEffect(() => { model.traverse((node) => { if ("castShadow" in node) { node.castShadow = true; node.receiveShadow = true; } }); }, [model]);
  useFrame((_, delta) => parts.forEach(({ part, y, offset }) => { const target = y + (exploded ? offset : 0); part.position.y = reducedMotion ? target : MathUtils.damp(part.position.y, target, 7, delta); }));
  return <group scale={scale || 1} rotation={rotation}><Center><primitive object={model} dispose={null} /></Center>{exploded && parts.map(({ part }) => <Html key={part.uuid} position={[part.position.x + 1, part.position.y, part.position.z]} center className="pointer-events-none whitespace-nowrap rounded-full bg-black/70 px-2 py-1 text-xs text-white">{ingredientPartNames[part.name]}</Html>)}</group>;
}
