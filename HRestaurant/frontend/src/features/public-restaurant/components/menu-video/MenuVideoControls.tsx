import { Pause, Play, Volume2, VolumeX } from "lucide-react";

export function VideoPlayButton({ playing, onClick }: { playing: boolean; onClick: () => void }) {
  const Icon = playing ? Pause : Play;
  return <button type="button" aria-label={playing ? "Videonu dayandır" : "Videonu oynat"} onClick={onClick} className="grid h-12 w-12 place-items-center rounded-full bg-[#c55232] text-white shadow-lg focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2"><Icon className="h-5 w-5" /></button>;
}

export function VideoMuteButton({ muted, onClick }: { muted: boolean; onClick: () => void }) {
  const Icon = muted ? VolumeX : Volume2;
  return <button type="button" aria-label={muted ? "Səsi aktiv et" : "Səsi söndür"} onClick={onClick} className="grid h-10 w-10 place-items-center rounded-full bg-black/65 text-white focus-visible:outline"><Icon className="h-4 w-4" /></button>;
}

export function MenuVideoControls(props: { playing: boolean; muted: boolean; onToggle: () => void; onMute: () => void }) {
  return <div className="absolute inset-x-3 bottom-3 flex items-end justify-between"><VideoPlayButton playing={props.playing} onClick={props.onToggle} /><VideoMuteButton muted={props.muted} onClick={props.onMute} /></div>;
}
