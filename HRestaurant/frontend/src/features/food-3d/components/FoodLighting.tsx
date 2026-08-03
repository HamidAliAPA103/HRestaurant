import { Environment } from "@react-three/drei";

export function FoodLighting() {
  return (
    <>
      <ambientLight intensity={0.7} />
      <hemisphereLight args={["#fff5e8", "#563c2d", 1.1]} />
      <directionalLight
        castShadow
        color="#fff0dc"
        intensity={2.4}
        position={[4, 7, 5]}
        shadow-mapSize={[1024, 1024]}
      />
      <spotLight
        color="#ffb58f"
        intensity={35}
        angle={0.35}
        penumbra={0.8}
        position={[-5, 4, -2]}
      />
      <Environment preset="studio" environmentIntensity={0.45} />
    </>
  );
}
