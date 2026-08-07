import { ContactShadows, Environment, OrbitControls } from "@react-three/drei";
import { Canvas, useFrame } from "@react-three/fiber";
import { Suspense, useRef } from "react";
import { Group, MathUtils } from "three";
import { FoodLoadingFallback } from "./FoodLoadingFallback";
import { FoodModel } from "./FoodModel";
import { SceneErrorBoundary } from "@/features/food-3d/components/SceneErrorBoundary";

function CameraRig({ progress }: { progress: React.MutableRefObject<number> }) { const camera = useRef<Group>(null); useFrame((state) => { const zoom = MathUtils.smoothstep(progress.current, .15, .3); state.camera.position.lerp({ x: 0, y: 1.8 - zoom * .25, z: 7 - zoom * 2.1 }, .08); state.camera.lookAt(0, .55, 0); }); return <group ref={camera} />; }
export function FoodScene({ progress, compact, onMissingMeshes, onModelError }: { progress: React.MutableRefObject<number>; compact: boolean; onMissingMeshes: (names: string[]) => void; onModelError: () => void }) {
  return <Canvas dpr={compact ? [1, 1.25] : [1, 1.75]} shadows={!compact} gl={{ antialias: !compact, powerPreference: "high-performance" }} camera={{ position: [0, 1.8, 7], fov: 36 }} onCreated={({ gl }) => gl.setClearColor("#160c10", 1)} fallback={<FoodLoadingFallback message="WebGL əlçatan deyil" />}>
    <ambientLight intensity={1.15} color="#f8dba0" /><spotLight position={[3.5, 6, 4]} angle={.45} penumbra={1} intensity={90} color="#ffe3a4" castShadow={!compact} /><pointLight position={[-4, 2, 2]} intensity={24} color="#a5482f" />
    <SceneErrorBoundary resetKey="/models/caesar-salad.glb" fallback={null} onError={onModelError}><Suspense fallback={<FoodLoadingFallback />}><FoodModel progress={progress} compact={compact} onMissingMeshes={onMissingMeshes} /></Suspense></SceneErrorBoundary>
    <ContactShadows position={[0, -1.25, 0]} opacity={.36} scale={8} blur={2.8} far={3.2} /><Environment preset="warehouse" /><CameraRig progress={progress} /><OrbitControls enablePan={false} enableZoom={false} enableRotate={false} /></Canvas>;
}
