import { Material, Texture } from "three";

type MaterialRecord = Material & Record<string, unknown>;

export function cloneMaterialWithTextures(material: Material) {
  const clone = material.clone() as MaterialRecord;
  Object.entries(clone).forEach(([key, value]) => {
    if (value instanceof Texture) clone[key] = value.clone();
  });
  return clone;
}

export function disposeMaterialAndTextures(material: Material) {
  Object.values(material).forEach((value) => {
    if (value instanceof Texture) value.dispose();
  });
  material.dispose();
}
