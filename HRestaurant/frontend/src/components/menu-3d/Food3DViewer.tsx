import { ContactShadows, Environment, OrbitControls, PerspectiveCamera } from "@react-three/drei";
import { Canvas } from "@react-three/fiber";
import { useReducedMotion } from "motion/react";
import { Suspense, useEffect, useRef, useState, type ComponentRef } from "react";
import { MathUtils } from "three";
import { FoodModel } from "./FoodModel";
import { FoodViewerControls } from "./FoodViewerControls";
import { FoodViewerErrorBoundary } from "./FoodViewerErrorBoundary";
import { FoodViewerLoader } from "./FoodViewerLoader";
import type { Food3DViewerProps } from "./types";

function Scene({ food, exploded, autoRotate, reducedMotion, resetVersion, onInteraction }: { food: Food3DViewerProps["food"]; exploded: boolean; autoRotate: boolean; reducedMotion: boolean; resetVersion: number; onInteraction: (active: boolean) => void }) {
  const controls = useRef<ComponentRef<typeof OrbitControls>>(null);
  useEffect(() => { controls.current?.reset(); }, [resetVersion]);
  return <><PerspectiveCamera makeDefault position={[4.5, 2.8, 5.4]} fov={40} /><ambientLight intensity={0.65} /><directionalLight castShadow position={[4, 7, 5]} intensity={2.2} shadow-mapSize={[1024, 1024]} /><Suspense fallback={<FoodViewerLoader />}><FoodModel url={food.model3DUrl!} scale={food.modelScale} rotation={[MathUtils.degToRad(food.modelRotationX), MathUtils.degToRad(food.modelRotationY), MathUtils.degToRad(food.modelRotationZ)]} exploded={exploded} reducedMotion={reducedMotion} /><Environment preset="studio" /><ContactShadows position={[0, -1.05, 0]} opacity={0.48} scale={8} blur={2.5} far={5} /></Suspense><OrbitControls ref={controls} makeDefault enableDamping enablePan={false} minDistance={2} maxDistance={10} autoRotate={autoRotate && !reducedMotion} autoRotateSpeed={0.85} onStart={() => onInteraction(true)} onEnd={() => onInteraction(false)} /></>;
}

export default function Food3DViewer({ food, onModelError }: Food3DViewerProps) {
  const reducedMotion = Boolean(useReducedMotion());
  const root = useRef<HTMLDivElement>(null);
  const [autoRotate, setAutoRotate] = useState(!reducedMotion);
  const [interacting, setInteracting] = useState(false);
  const [exploded, setExploded] = useState(false);
  const [resetVersion, setResetVersion] = useState(0);
  const [visible, setVisible] = useState(!document.hidden);
  useEffect(() => { const update = () => setVisible(!document.hidden); document.addEventListener("visibilitychange", update); return () => document.removeEventListener("visibilitychange", update); }, []);
  const fullscreen = () => { if (!document.fullscreenElement) void root.current?.requestFullscreen(); else void document.exitFullscreen(); };
  return <FoodViewerErrorBoundary onError={onModelError} fallback={<div role="alert" className="grid h-full place-items-center bg-[#211914] text-white">3D model göstərilə bilmədi</div>}><div ref={root} className="relative h-full min-h-[24rem] overflow-hidden bg-[radial-gradient(circle_at_50%_25%,#fff8ec,#d8baa0_60%,#92715c)]"><Canvas shadows dpr={[1, 1.5]} frameloop={visible ? "always" : "demand"} gl={{ antialias: true, alpha: true, powerPreference: "high-performance" }} aria-label={`${food.name} interaktiv 3D modeli. Modeli döndərin və yaxınlaşdırın.`}><Scene food={food} exploded={exploded} autoRotate={autoRotate && !interacting && visible} reducedMotion={reducedMotion} resetVersion={resetVersion} onInteraction={setInteracting} /></Canvas><div className="absolute left-4 top-4 rounded-full bg-black/65 px-3 py-1.5 text-xs font-bold text-white backdrop-blur">Modeli döndərin · Yaxınlaşdırın</div>{food.enableIngredientAnimation && <button type="button" onClick={() => setExploded((value) => !value)} className="absolute right-4 top-4 rounded-full bg-[#c55232] px-4 py-2 text-xs font-bold text-white shadow-lg">{exploded ? "Yeməyi birləşdir" : "İnqrediyentləri göstər"}</button>}<FoodViewerControls autoRotate={autoRotate} reducedMotion={reducedMotion} onAutoRotate={() => setAutoRotate((value) => !value)} onReset={() => setResetVersion((value) => value + 1)} onFullscreen={fullscreen} /></div></FoodViewerErrorBoundary>;
}
