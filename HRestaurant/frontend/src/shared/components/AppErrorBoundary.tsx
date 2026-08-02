import { Component, type ReactNode } from "react";
import { AlertTriangle } from "lucide-react";

interface State { failed: boolean }

export class AppErrorBoundary extends Component<{ children: ReactNode }, State> {
  state: State = { failed: false };

  static getDerivedStateFromError(): State {
    return { failed: true };
  }

  render() {
    if (!this.state.failed) return this.props.children;
    return <main className="grid min-h-screen place-items-center bg-[#f6f2eb] p-4 text-center"><section className="max-w-md rounded-3xl bg-white p-8 shadow-xl"><AlertTriangle className="mx-auto h-10 w-10 text-[#d64f34]" /><h1 className="mt-5 text-3xl font-bold">Səhifə göstərilə bilmədi</h1><p className="mt-3 text-sm leading-6 text-[#756b63]">Gözlənilməz interfeys xətası baş verdi. Səhifəni yeniləyərək təhlükəsiz şəkildə davam edin.</p><button type="button" onClick={() => window.location.reload()} className="mt-6 rounded-xl bg-[#e85d3f] px-5 py-3 font-bold text-white">Səhifəni yenilə</button></section></main>;
  }
}
