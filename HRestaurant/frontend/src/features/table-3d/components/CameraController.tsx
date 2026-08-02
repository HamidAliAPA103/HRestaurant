import { OrbitControls } from "@react-three/drei";
import { useThree } from "@react-three/fiber";
import { useEffect, useMemo } from "react";
import type { PublicRestaurantTable } from "@/types/public";

export function CameraController({ tables }: { tables: PublicRestaurantTable[] }) {
  const { camera, invalidate } = useThree();
  const frame = useMemo(() => {
    if (!tables.length) return { x: 0, z: 0, distance: 15 };
    const minX = Math.min(...tables.map((table) => table.positionX - table.width));
    const maxX = Math.max(...tables.map((table) => table.positionX + table.width));
    const minZ = Math.min(...tables.map((table) => table.positionZ - table.length));
    const maxZ = Math.max(...tables.map((table) => table.positionZ + table.length));
    return { x: (minX + maxX) / 2, z: (minZ + maxZ) / 2, distance: Math.max(11, maxX - minX, maxZ - minZ) * 1.25 };
  }, [tables]);
  useEffect(() => {
    camera.position.set(frame.x, frame.distance * .72, frame.z + frame.distance);
    camera.lookAt(frame.x, 0, frame.z);
    camera.updateProjectionMatrix(); invalidate();
  }, [camera, frame, invalidate]);
  return <OrbitControls makeDefault enableDamping dampingFactor={.08} minDistance={6} maxDistance={Math.max(30, frame.distance * 2)} maxPolarAngle={Math.PI / 2.05} target={[frame.x, .5, frame.z]} />;
}
