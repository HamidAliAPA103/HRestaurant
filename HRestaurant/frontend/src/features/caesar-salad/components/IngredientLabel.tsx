import { Html } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { useRef } from "react";
import { Group } from "three";
import type { CaesarIngredient } from "../types";

export function IngredientLabel({ ingredient, progress, compact }: { ingredient: CaesarIngredient; progress: React.MutableRefObject<number>; compact: boolean }) {
  const ref = useRef<Group>(null);
  useFrame(() => { if (ref.current) ref.current.visible = progress.current > 0.69; });
  const [x, y, z] = ingredient.explodedPosition;
  return <group ref={ref} position={[x, y + 0.22, z]}><Html center distanceFactor={compact ? 7.5 : 6} transform={false} style={{ pointerEvents: "none", opacity: progress.current > 0.75 ? 1 : 0 }}><span className="block whitespace-nowrap rounded-full border border-[#e3c281]/40 bg-[#190c11]/90 px-2.5 py-1 text-[10px] font-bold uppercase tracking-[.13em] text-[#f5dfac] shadow-lg">{compact ? ingredient.name.split(" ")[0] : ingredient.name}</span></Html></group>;
}
