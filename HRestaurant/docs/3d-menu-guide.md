# 3D menyu bələdçisi

## Model hazırlığı

1. Image-to-3D alətinə yaxşı işıqlanmış ön, yan və arxa yemək fotoları verin. Nəticəni yalnız başlanğıc mesh kimi qəbul edin; kommersiya istifadəsi üçün alətin lisenziyasını yoxlayın.
2. Blender-də mesh səhvlərini, gizli səthləri və lazımsız hissələri silin. Normalları düzəldin, origin-i yeməyin mərkəzinə, alt səthi isə `Y=0`-a gətirin. Transform-ları tətbiq edin.
3. Ayrı animasiya tələb olunan hissələri semantik adlandırın (`TopBun`, `Lettuce`, `Tomato`, `Cheese`, `Patty`, `Sauce`, `BottomBun`). Adlar olmayan model normal şəkildə işləyəcək, sadəcə exploded view göstərməyəcək.
4. PBR materiallardan istifadə edin. Mobil üçün 1K–2K tekstura adətən kifayətdir; yalnız yaxın plan həqiqətən tələb edirsə 4K istifadə edin. Base Color sRGB, normal/roughness/metallic isə non-color olmalıdır.
5. Silueti qoruyaraq decimate/retopology edin. Mobil başlanğıc hədəfi təxminən 50–150 min üçbucaqdır. Kiçik detalları normal map-ə bake edin.
6. `File > Export > glTF 2.0`, format `GLB`, materiallar və teksturalar daxil, +Y up parametrləri ilə ixrac edin. Blender-in glTF validator xəbərdarlıqlarını həll edin.
7. `gltf-transform optimize input.glb output.glb --compress draco` və ya Meshopt istifadə edin. Sıxılmış və sıxılmamış variantı real iOS/Android cihazlarında yoxlayın; istehsal faylını ideal olaraq 5–8 MB-dan aşağı saxlayın.

## 360° video

Blender-də kameranı sabit saxlayıb modeli və ya turntable platformasını 360° fırladın. 6–10 saniyəlik problemsiz loop render edin. WebM/VP9 və əlavə uyğunluq lazım olduqda MP4/H.264 hazırlayın. Poster ilk kadrla vizual olaraq uyğun olmalıdır.

## Faylların yerləşdirilməsi

- GLB: `frontend/public/models/menu/`
- WebM/MP4: `frontend/public/videos/menu/`
- WebP poster: `frontend/public/images/menu/`

CDN istifadə edilirsə CORS başlıqlarına, düzgün MIME tiplərinə (`model/gltf-binary`, `video/webm`, `image/webp`) və uzunmüddətli cache/versioned URL-lərə diqqət edin.

## Admin qoşulması

Menyu idarəetməsində məhsulu yaradın və ya redaktə edin. “3D görünüşü aktiv et” seçin, GLB və poster URL-lərini yazın, lazım olduqda miqyas/rotasiyanı sazlayın. Named-node model üçün “İnqrediyent animasiyasını aktiv et” seçin. Video fallback üçün video və poster URL-i əlavə edib video təqdimatını aktivləşdirin.

İlk burger üçün nümunə URL-lər:

```text
/models/menu/classic-burger.glb
/videos/menu/classic-burger.webm
/images/menu/classic-burger-poster.webp
```

## Tipik problemlər

- Qara model: işıqlandırma, normallar və tekstura color-space parametrlərini yoxlayın.
- Həddən artıq böyük/kiçik model: Blender transform-larını apply edin və admin miqyasını sazlayın.
- Çəhrayı və ya itmiş tekstura: GLB-yə teksturaların embed edildiyini yoxlayın.
- Model açılmır: şəbəkə/CORS/MIME cavabını və glTF Validator nəticəsini yoxlayın.
- Mobil donma: poliqon, draw call, tekstura ölçüsü və material sayını azaldın.
- Exploded view işləmir: node adlarını və “İnqrediyent animasiyası” seçimini yoxlayın.
