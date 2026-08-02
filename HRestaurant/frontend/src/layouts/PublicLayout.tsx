import { Menu, X } from "lucide-react";
import { useState } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";

export function PublicLayout() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();
  const slug = location.pathname.match(/^\/restaurants\/([^/]+)/)?.[1];
  const links = [
    { label: "Ana səhifə", to: "/" },
    ...(slug ? [
      { label: "Restoran", to: `/restaurants/${slug}` },
      { label: "Menyu", to: `/restaurants/${slug}/menu` },
      { label: "Rezervasiya", to: `/restaurants/${slug}/reservation` },
    ] : []),
    { label: "Rezervasiyanı yoxla", to: "/reservation/track" },
  ];
  return <div className="min-h-screen bg-[#f7f3ec] text-[#241f1a]">
    <header className="sticky top-0 z-40 border-b border-black/5 bg-[#f7f3ec]/90 backdrop-blur-xl"><div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8"><Link to="/" className="flex items-center gap-3 rounded-full focus:outline-none focus-visible:ring-2 focus-visible:ring-[#b5422d]"><span className="grid h-10 w-10 place-items-center rounded-full bg-[#242019] font-serif text-lg text-white">H</span><span><span className="block font-serif text-lg leading-none">HRestaurant</span><span className="text-[10px] uppercase tracking-[0.24em] text-[#81766d]">Reserve your table</span></span></Link><nav aria-label="Əsas naviqasiya" className="hidden items-center gap-1 md:flex">{links.map((link) => <Link key={link.to} to={link.to} className={`rounded-full px-4 py-2 text-sm font-semibold ${location.pathname === link.to ? "bg-[#211d18] text-white" : "hover:bg-white"}`}>{link.label}</Link>)}</nav><button type="button" aria-label={mobileOpen ? "Menyunu bağla" : "Menyunu aç"} onClick={() => setMobileOpen((value) => !value)} className="grid h-10 w-10 place-items-center rounded-full border bg-white md:hidden">{mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}</button></div>{mobileOpen && <nav aria-label="Mobil naviqasiya" className="border-t bg-white p-3 md:hidden">{links.map((link) => <Link key={link.to} to={link.to} onClick={() => setMobileOpen(false)} className="block rounded-xl px-4 py-3 text-sm font-bold hover:bg-[#f5f0e9]">{link.label}</Link>)}</nav>}</header>
    <main><Outlet /></main>
    <footer className="border-t border-black/5 bg-[#211d18] px-4 py-10 text-white/60"><div className="mx-auto flex max-w-7xl flex-col gap-3 text-sm sm:flex-row sm:items-center sm:justify-between"><div><p className="font-serif text-xl text-white">HRestaurant</p><p className="mt-1">Təhlükəsiz rezervasiya · Şəxsi məlumatlarınız qorunur</p></div><div className="flex gap-4"><Link to="/" className="hover:text-white">Restoranlar</Link><Link to="/reservation/track" className="hover:text-white">Rezervasiya izləmə</Link></div><p>© {new Date().getFullYear()} HRestaurant</p></div></footer>
  </div>;
}
