import { useEffect, useRef, useState } from "react";
import type { PublicMenuItem } from "@/types/public";
import { MenuVideoControls } from "./MenuVideoControls";

interface Props {
  item: PublicMenuItem;
  activeVideoId: string | null;
  onActiveChange: (id: string | null) => void;
}

export function MenuVideoPlayer({ item, activeVideoId, onActiveChange }: Props) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const rootRef = useRef<HTMLDivElement>(null);
  const [nearViewport, setNearViewport] = useState(false);
  const [muted, setMuted] = useState(true);
  const [failed, setFailed] = useState(false);
  const playing = activeVideoId === item.id;

  useEffect(() => {
    const node = rootRef.current;
    if (!node) return;
    const observer = new IntersectionObserver(([entry]) => {
      setNearViewport(entry.isIntersecting);
      if (!entry.isIntersecting && activeVideoId === item.id) onActiveChange(null);
    }, { rootMargin: "240px" });
    observer.observe(node);
    return () => observer.disconnect();
  }, [activeVideoId, item.id, onActiveChange]);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;
    if (playing) void video.play().catch(() => onActiveChange(null));
    else { video.pause(); video.currentTime = 0; }
    return () => video.pause();
  }, [onActiveChange, playing]);

  const toggle = () => onActiveChange(playing ? null : item.id);
  const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)").matches;

  return (
    <div ref={rootRef} className="relative aspect-video overflow-hidden bg-[#211914]"
      onMouseEnter={() => { if (!reducedMotion && matchMedia("(hover: hover)").matches) onActiveChange(item.id); }}
      onMouseLeave={() => { if (activeVideoId === item.id) onActiveChange(null); }}>
      <img loading="lazy" src={item.videoPosterUrl || item.imageUrl || undefined} alt={`${item.name} posteri`} className="absolute inset-0 h-full w-full object-cover" />
      {nearViewport && !failed && item.videoUrl && (
        <video ref={videoRef} src={item.videoUrl} poster={item.videoPosterUrl || item.imageUrl || undefined}
          muted={muted} playsInline loop preload="metadata" onError={() => setFailed(true)}
          className={`absolute inset-0 h-full w-full object-cover transition-opacity ${playing ? "opacity-100" : "opacity-0"}`} />
      )}
      <div className="absolute inset-0 bg-gradient-to-t from-black/75 via-transparent to-black/20" />
      <MenuVideoControls playing={playing} muted={muted} onToggle={toggle} onMute={() => setMuted((value) => !value)} />
      {item.videoDurationSeconds != null && <span className="absolute right-3 top-3 rounded-full bg-black/70 px-2 py-1 text-xs text-white">{Math.floor(item.videoDurationSeconds / 60)}:{String(item.videoDurationSeconds % 60).padStart(2, "0")}</span>}
    </div>
  );
}
