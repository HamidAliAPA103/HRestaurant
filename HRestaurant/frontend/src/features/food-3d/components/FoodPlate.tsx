import { memo } from "react";

export const FoodPlate = memo(function FoodPlate() {
  return (
    <group position={[0, 0.08, 0]}>
      <mesh receiveShadow>
        <cylinderGeometry args={[2.05, 1.88, 0.12, 72]} />
        <meshStandardMaterial color="#f4eee5" roughness={0.2} metalness={0.02} />
      </mesh>
      <mesh position={[0, 0.08, 0]} receiveShadow>
        <torusGeometry args={[1.73, 0.09, 24, 72]} />
        <meshStandardMaterial color="#d8c8b6" roughness={0.35} />
      </mesh>
    </group>
  );
});
