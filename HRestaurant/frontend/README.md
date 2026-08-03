# HRestaurant Frontend

React, TypeScript, Vite və React Three Fiber ilə hazırlanmış idarəetmə və public restoran tətbiqi.

## Lokal işə salma

1. `.env.example` faylını `.env` kimi kopyalayın.
2. Backend API-ni işə salın.
3. `npm.cmd run dev` ilə frontend-i başladın.

Production build eyni-origin `/api` ünvanından istifadə edir.

## Blender export və GLB optimizasiya qaydaları

- Blender vahidini metr saxlayın, transformları tətbiq edin və origin-i vizual mərkəzə yerləşdirin.
- Yalnız görünən mesh-ləri export edin; kamera, helper işıqlar və gizli high-poly obyektləri çıxarın.
- Yemək modeli üçün 50–120 min, ingredient üçün 5–20 min triangle büdcəsi istifadə edin. Təkrarlanan dekorları eyni mesh və materialla instancing üçün hazırlayın.
- GLB export-da Draco compression aktiv edin. Sonra `gltf-transform optimize input.glb output.glb --compress meshopt --texture-compress webp` işlədin.
- Rəng teksturalarını AVIF/WebP, GPU teksturalarını KTX2/Basis Universal edin. Desktop maksimum 2048px, mobil maksimum 1024px olsun.
- Eyni kamera bucağından AVIF və WebP poster yaradın. Model və ya WebGL xətasında bu poster göstərilir.
- Material və draw-call sayını azaldın; şəffaf material və 4K teksturanı yalnız ölçülmüş vizual fayda olduqda saxlayın.
- Versiyalı CDN URL-i və düzgün `model/gltf-binary`, AVIF, WebP, KTX2 MIME tipləri istifadə edin. KTX2 transcoder fayllarını `/basis/` altında deploy edin.
- Modeli API-yə yalnız aşağı səviyyəli mobil cihazda LCP, GPU memory və interaction testi keçdikdən sonra əlavə edin.

## 3D performans profilləri

High, Medium və Low profilləri touch dəstəyi, viewport, CPU nüvəsi, cihaz yaddaşı, pixel ratio və reduced-motion seçiminə görə avtomatik seçilir. Low profildə DPR maksimum 1, kölgə və postprocessing deaktiv, işıq və particle büdcəsi az, materiallar sadə və frameloop demand-dır. WebGL olmadıqda menyu, rezervasiya və əlçatan HTML siyahıları işləməyə davam edir.
