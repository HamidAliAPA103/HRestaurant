export type PerformanceLevel = "high" | "medium" | "low";
export interface PerformanceProfile { level: PerformanceLevel; dpr: [number, number]; shadows: boolean; particleCount: number; simpleMaterials: boolean; postprocessing: boolean; dynamicLights: number; frameloop: "always" | "demand"; }
interface CapabilityInput { mobile: boolean; reducedMotion: boolean; cores: number; memory?: number; pixelRatio: number; }
export function selectPerformanceLevel(input: CapabilityInput): PerformanceLevel {
  if (input.reducedMotion || input.mobile || input.cores <= 4 || (input.memory !== undefined && input.memory <= 4)) return "low";
  if (input.cores <= 8 || input.pixelRatio > 2) return "medium";
  return "high";
}
export function createPerformanceProfile(level: PerformanceLevel): PerformanceProfile {
  if (level === "low") return { level, dpr: [1, 1], shadows: false, particleCount: 18, simpleMaterials: true, postprocessing: false, dynamicLights: 1, frameloop: "demand" };
  if (level === "medium") return { level, dpr: [1, 1.5], shadows: true, particleCount: 42, simpleMaterials: false, postprocessing: false, dynamicLights: 2, frameloop: "always" };
  return { level, dpr: [1, 2], shadows: true, particleCount: 72, simpleMaterials: false, postprocessing: true, dynamicLights: 3, frameloop: "always" };
}
export function detectPerformanceProfile(reducedMotion: boolean): PerformanceProfile {
  if (typeof window === "undefined") return createPerformanceProfile("medium");
  const nav = navigator as Navigator & { deviceMemory?: number };
  return createPerformanceProfile(selectPerformanceLevel({ mobile: matchMedia("(pointer: coarse)").matches || innerWidth < 768, reducedMotion, cores: navigator.hardwareConcurrency || 4, memory: nav.deviceMemory, pixelRatio: devicePixelRatio || 1 }));
}
