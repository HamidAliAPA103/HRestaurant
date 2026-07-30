import { Link, Outlet } from "react-router-dom";

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-[#f7f3ec] text-[#241f1a]">
      <header className="sticky top-0 z-40 border-b border-black/5 bg-[#f7f3ec]/90 backdrop-blur-xl">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
          <Link
            to="/"
            className="flex items-center gap-3 rounded-full focus:outline-none focus-visible:ring-2 focus-visible:ring-[#b5422d]"
          >
            <span className="grid h-10 w-10 place-items-center rounded-full bg-[#242019] font-serif text-lg text-white">
              H
            </span>
            <span>
              <span className="block font-serif text-lg leading-none">
                HRestaurant
              </span>
              <span className="text-[10px] uppercase tracking-[0.24em] text-[#81766d]">
                Reserve your table
              </span>
            </span>
          </Link>
          <Link
            to="/reservation/track"
            className="rounded-full border border-[#d7cec2] bg-white px-4 py-2 text-sm font-semibold transition hover:border-[#b5422d] hover:text-[#b5422d] focus:outline-none focus-visible:ring-2 focus-visible:ring-[#b5422d]"
          >
            Rezervasiyanı yoxla
          </Link>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
      <footer className="border-t border-black/5 bg-[#211d18] px-4 py-8 text-center text-sm text-white/60">
        Təhlükəsiz rezervasiya · Şəxsi məlumatlarınız qorunur
      </footer>
    </div>
  );
}
