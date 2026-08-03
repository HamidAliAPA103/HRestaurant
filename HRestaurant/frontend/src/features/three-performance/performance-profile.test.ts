import { describe, expect, it } from "vitest";
import { createPerformanceProfile, selectPerformanceLevel } from "./performance-profile";
describe("3D performance profiles", () => {
  it("caps mobile at low", () => { expect(selectPerformanceLevel({ mobile: true, reducedMotion: false, cores: 12, memory: 16, pixelRatio: 3 })).toBe("low"); expect(createPerformanceProfile("low")).toMatchObject({ dpr: [1, 1], shadows: false, postprocessing: false }); });
  it("selects high for capable desktop", () => expect(selectPerformanceLevel({ mobile: false, reducedMotion: false, cores: 12, memory: 16, pixelRatio: 1.5 })).toBe("high"));
  it("honours reduced motion", () => expect(selectPerformanceLevel({ mobile: false, reducedMotion: true, cores: 12, memory: 16, pixelRatio: 1 })).toBe("low"));
});
