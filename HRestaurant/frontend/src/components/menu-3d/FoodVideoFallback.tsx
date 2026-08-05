interface Props { name: string; videoUrl?: string | null; posterUrl?: string | null; message?: string }
export function FoodVideoFallback({ name, videoUrl, posterUrl, message }: Props) {
  if (videoUrl) return <video className="h-full w-full object-contain" controls playsInline preload="metadata" poster={posterUrl ?? undefined} aria-label={`${name} üçün 360° video`}><source src={videoUrl} />Brauzeriniz videonu dəstəkləmir.</video>;
  return <div className="relative grid h-full place-items-center overflow-hidden bg-[#211914] text-center text-white">{posterUrl && <img src={posterUrl} alt={`${name} görünüşü`} className="absolute inset-0 h-full w-full object-cover" />}<div className="absolute inset-0 bg-black/55" /><p role="alert" className="relative max-w-md px-8 text-lg font-semibold">{message ?? "3D model göstərilə bilmədi"}</p></div>;
}
