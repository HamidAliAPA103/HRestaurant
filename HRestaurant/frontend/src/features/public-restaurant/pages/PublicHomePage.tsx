import { useQuery } from "@tanstack/react-query";
import gsap from "gsap";
import { ArrowDownRight, ArrowRight, MapPin, RefreshCw, Sparkles, Store } from "lucide-react";
import { useReducedMotion } from "motion/react";
import { useLayoutEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { getPublicApiError, getPublicRestaurants } from "@/api/public-api";
import { MagneticLink, Reveal } from "@/features/public-design/components/PublicExperience";

export function PublicHomePage() {
  const query = useQuery({ queryKey: ["public-restaurants"], queryFn: getPublicRestaurants });
  const hero = useRef<HTMLElement>(null);
  const reduced = Boolean(useReducedMotion());
  useLayoutEffect(() => {
    if (!hero.current || reduced) return;
    const context = gsap.context(() => gsap.from("[data-hero-reveal]", { y: 70, opacity: 0, stagger: .13, duration: 1, ease: "power4.out" }), hero);
    return () => context.revert();
  }, [reduced]);
  if (query.isLoading) return <div aria-live="polite" className="mx-auto min-h-[70vh] max-w-[90rem] animate-pulse px-4 py-20"><span className="sr-only">Restoranlar yüklənir</span><div className="h-20 max-w-2xl rounded-3xl bg-black/10" /></div>;
  if (query.isError) return <div className="grid min-h-[70vh] place-items-center px-4 text-center"><div><RefreshCw className="mx-auto text-accent-primary" /><h1 className="display-type mt-5 text-5xl">Restoranlar yüklənmədi</h1><p className="mt-3 text-text-secondary">{getPublicApiError(query.error).message}</p><button onClick={() => query.refetch()} className="mt-6 rounded-full bg-accent-primary px-6 py-3 font-bold text-white">Yenidən cəhd et</button></div></div>;
  const featured = query.data?.[0];
  return <>
    <section ref={hero} className="relative isolate min-h-[calc(100svh-76px)] overflow-hidden bg-background-secondary px-4 py-16 text-white sm:px-6 lg:px-10">
      {featured?.coverImageUrl && <img src={featured.coverImageUrl} alt="" className="absolute inset-0 -z-20 h-full w-full object-cover opacity-30" />}
      <div className="absolute inset-0 -z-10 bg-[linear-gradient(90deg,rgba(28,24,21,.98)_0%,rgba(28,24,21,.82)_48%,rgba(111,21,36,.35)_100%)]" />
      <div className="light-reflection absolute -right-20 top-10 h-[32rem] w-[32rem] rounded-full bg-accent-secondary/20 blur-3xl" />
      <div className="mx-auto grid min-h-[72svh] max-w-[90rem] items-end gap-10 lg:grid-cols-[1.25fr_.75fr]">
        <div className="pb-8"><p data-hero-reveal className="flex items-center gap-2 text-xs font-bold uppercase tracking-[.32em] text-gold"><Sparkles className="h-4 w-4" /> Yeni nəsil restoran təcrübəsi</p><h1 data-hero-reveal className="display-type mt-7 max-w-5xl text-[clamp(4rem,9vw,9rem)] leading-[.78]">Bir masa.<br/><em className="font-normal text-[#d96b43]">Sonsuz xatirə.</em></h1><p data-hero-reveal className="mt-9 max-w-xl text-base leading-8 text-white/65 sm:text-lg">Menyunu hiss edin, restoranı 3D gəzin və mükəmməl masanızı bir neçə toxunuşla seçin.</p><div data-hero-reveal className="mt-9 flex flex-wrap gap-4">{featured && <MagneticLink href={`/restaurants/${featured.slug}`} className="inline-flex items-center gap-3 rounded-full bg-accent-secondary px-7 py-4 font-bold text-white shadow-2xl shadow-accent-secondary/20">Kəşf etməyə başla <ArrowRight /></MagneticLink>}<MagneticLink href="#restaurants" className="inline-flex items-center gap-3 rounded-full border border-white/25 px-7 py-4 font-bold text-white">Restoranları seç <ArrowDownRight /></MagneticLink></div></div>
        <aside className="glass mb-8 rounded-[2rem] p-6 text-text-primary lg:ml-auto lg:max-w-sm"><p className="text-xs font-bold uppercase tracking-[.22em] text-accent-primary">Canlı təcrübə</p><div className="mt-5 grid grid-cols-2 gap-3"><div className="rounded-2xl bg-white/55 p-4"><strong className="display-type text-3xl">3D</strong><span className="mt-1 block text-xs text-text-secondary">Menyu və interyer</span></div><div className="rounded-2xl bg-white/55 p-4"><strong className="display-type text-3xl">Live</strong><span className="mt-1 block text-xs text-text-secondary">Masa uyğunluğu</span></div></div></aside>
      </div>
    </section>
    <section id="restaurants" className="mx-auto max-w-[90rem] px-4 py-24 sm:px-6 lg:px-10 lg:py-32"><Reveal><p className="text-xs font-bold uppercase tracking-[.3em] text-accent-primary">Məkanlarımız</p><div className="mt-4 flex flex-col justify-between gap-4 md:flex-row md:items-end"><h2 className="display-type max-w-3xl text-5xl leading-none sm:text-7xl">Axşamınız üçün doğru atmosfer.</h2><p className="max-w-sm leading-7 text-text-secondary">Hər məkanın öz ritmi, menyusu və hekayəsi var.</p></div></Reveal>
      {!query.data?.length ? <div className="mt-12 rounded-[2rem] border border-black/10 p-12 text-center"><Store className="mx-auto" /><p className="mt-3">Hazırda public restoran yoxdur.</p></div> : <div className="mt-14 grid gap-7 md:grid-cols-2 xl:grid-cols-3">{query.data.map((restaurant, index) => <Reveal key={restaurant.id} delay={index * .08}><article className="group relative min-h-[32rem] overflow-hidden rounded-[2.25rem] bg-background-secondary text-white"><div className="absolute inset-0">{restaurant.coverImageUrl ? <img src={restaurant.coverImageUrl} alt={`${restaurant.name} interyeri`} className="h-full w-full object-cover transition duration-700 group-hover:scale-105" /> : <div className="h-full bg-[radial-gradient(circle_at_top,#6f1524,#1c1815)]" />}<div className="absolute inset-0 bg-gradient-to-t from-black via-black/20 to-transparent" /></div><div className="absolute inset-x-0 bottom-0 p-7"><span className={`rounded-full px-3 py-1 text-xs font-bold ${restaurant.isOpenNow ? "bg-success text-white" : "bg-white/15"}`}>{restaurant.isOpenNow ? "Açıqdır" : "Bağlıdır"}</span><h3 className="display-type mt-4 text-4xl">{restaurant.name}</h3><p className="mt-2 flex items-center gap-2 text-sm text-white/65"><MapPin className="h-4 w-4 text-gold" />{restaurant.address}</p><Link to={`/restaurants/${restaurant.slug}`} className="mt-6 inline-flex items-center gap-2 font-bold text-gold">Məkanı kəşf et <ArrowRight className="transition group-hover:translate-x-1" /></Link></div></article></Reveal>)}</div>}
    </section>
  </>;
}
