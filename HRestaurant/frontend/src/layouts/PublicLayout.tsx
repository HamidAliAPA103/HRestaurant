import { AnimatePresence, motion } from "motion/react";
import { Menu, X } from "lucide-react";
import { useState } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { PageTransition, PublicExperience } from "@/features/public-design/components/PublicExperience";

export function PublicLayout() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();
  const slug = location.pathname.match(/^\/restaurants\/([^/]+)/)?.[1];
  const links = [{ label: "Ana səhifə", to: "/" }, ...(slug ? [
    { label: "Restoran", to: `/restaurants/${slug}` }, { label: "Menyu", to: `/restaurants/${slug}/menu` },
    { label: "3D təcrübə", to: `/restaurants/${slug}/experience` }, { label: "Rezervasiya", to: `/restaurants/${slug}/reservation` },
  ] : []), { label: "Rezervasiyanı yoxla", to: "/reservation/track" }];
  if (location.pathname === "/") return <Outlet />;
  return <PublicExperience>
    <header className="sticky top-0 z-50 border-b border-black/5 bg-background-primary/80 backdrop-blur-2xl">
      <div className="mx-auto flex max-w-[90rem] items-center justify-between px-4 py-4 sm:px-6 lg:px-10">
        <Link to="/" className="flex items-center gap-3 rounded-full"><span className="grid h-11 w-11 place-items-center rounded-full bg-accent-primary display-type text-xl text-white">H</span><span><strong className="display-type block text-xl leading-none">HRestaurant</strong><small className="text-[9px] uppercase tracking-[.28em] text-text-secondary">Dining, reimagined</small></span></Link>
        <nav aria-label="Əsas naviqasiya" className="hidden items-center gap-6 md:flex">{links.map((link) => <Link key={link.to} to={link.to} aria-current={location.pathname === link.to ? "page" : undefined} className="animated-underline py-2 text-xs font-bold uppercase tracking-[.13em] text-text-secondary transition hover:text-accent-primary">{link.label}</Link>)}</nav>
        <button type="button" aria-expanded={mobileOpen} aria-controls="public-mobile-menu" aria-label={mobileOpen ? "Menyunu bağla" : "Menyunu aç"} onClick={() => setMobileOpen(!mobileOpen)} className="grid h-11 w-11 place-items-center rounded-full border border-black/10 md:hidden">{mobileOpen ? <X /> : <Menu />}</button>
      </div>
      <AnimatePresence>{mobileOpen && <motion.nav id="public-mobile-menu" aria-label="Mobil naviqasiya" initial={{ height: 0, opacity: 0 }} animate={{ height: "auto", opacity: 1 }} exit={{ height: 0, opacity: 0 }} className="overflow-hidden border-t bg-background-primary md:hidden"><div className="space-y-1 p-4">{links.map((link) => <Link key={link.to} to={link.to} onClick={() => setMobileOpen(false)} className="block rounded-2xl px-4 py-4 font-semibold hover:bg-white/70">{link.label}</Link>)}</div></motion.nav>}</AnimatePresence>
    </header>
    <AnimatePresence mode="wait"><PageTransition key={location.pathname}><main><Outlet /></main></PageTransition></AnimatePresence>
    <footer className="bg-background-secondary px-4 py-14 text-white/60"><div className="mx-auto grid max-w-[90rem] gap-8 sm:grid-cols-3"><div><p className="display-type text-3xl text-white">HRestaurant</p><p className="mt-2 text-sm">Dad, məkan və xatirə — bir masada.</p></div><nav aria-label="Alt naviqasiya" className="flex flex-col gap-2 text-sm"><Link to="/">Restoranlar</Link><Link to="/reservation/track">Rezervasiyanı izləyin</Link></nav><p className="text-sm sm:text-right">© {new Date().getFullYear()} HRestaurant</p></div></footer>
  </PublicExperience>;
}
