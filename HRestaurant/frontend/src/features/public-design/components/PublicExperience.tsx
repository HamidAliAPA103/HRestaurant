import Lenis from "lenis";
import gsap from "gsap";
import { ScrollTrigger } from "gsap/ScrollTrigger";
import { motion, useReducedMotion, useScroll, useSpring } from "motion/react";
import { useEffect, useState, type ReactNode } from "react";

gsap.registerPlugin(ScrollTrigger);

export function PublicExperience({ children }: { children: ReactNode }) {
  const reduced = Boolean(useReducedMotion());
  const { scrollYProgress } = useScroll();
  const progress = useSpring(scrollYProgress, { stiffness: 130, damping: 28, mass: 0.2 });
  const [pointer, setPointer] = useState({ x: -40, y: -40, active: false });

  useEffect(() => {
    if (reduced) return;
    const lenis = new Lenis({ duration: 1.05, smoothWheel: true });
    const update = (time: number) => lenis.raf(time * 1000);
    lenis.on("scroll", ScrollTrigger.update);
    gsap.ticker.add(update);
    gsap.ticker.lagSmoothing(0);
    return () => { gsap.ticker.remove(update); lenis.destroy(); };
  }, [reduced]);

  return <div className="public-shell public-noise min-h-screen" onPointerMove={(event) => setPointer({ x: event.clientX, y: event.clientY, active: true })} onPointerLeave={() => setPointer((value) => ({ ...value, active: false }))}>
    <motion.div aria-hidden className="scroll-progress fixed inset-x-0 top-0 z-[80] h-1 bg-gold" style={{ scaleX: progress }} />
    <motion.div aria-hidden className="pointer-events-none fixed left-0 top-0 z-[90] hidden h-5 w-5 rounded-full border border-gold mix-blend-difference lg:block" animate={{ x: pointer.x - 10, y: pointer.y - 10, opacity: pointer.active ? 1 : 0 }} transition={{ type: "spring", stiffness: 520, damping: 34, mass: .12 }} />
    {children}
  </div>;
}

export function PageTransition({ children }: { children: ReactNode }) {
  const reduced = Boolean(useReducedMotion());
  return <motion.div initial={reduced ? false : { opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }} exit={reduced ? undefined : { opacity: 0, y: -8 }} transition={{ duration: reduced ? 0 : .45, ease: [.22, 1, .36, 1] }}>{children}</motion.div>;
}

export function Reveal({ children, className = "", delay = 0 }: { children: ReactNode; className?: string; delay?: number }) {
  const reduced = Boolean(useReducedMotion());
  return <motion.div className={className} initial={reduced ? false : { opacity: 0, y: 28 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true, amount: .18 }} transition={{ duration: reduced ? 0 : .65, delay }}>{children}</motion.div>;
}

export function MagneticLink({ children, className = "", href }: { children: ReactNode; className?: string; href: string }) {
  const reduced = Boolean(useReducedMotion());
  const [offset, setOffset] = useState({ x: 0, y: 0 });
  return <motion.a href={href} className={className} animate={offset} onPointerMove={(event) => { if (reduced || event.pointerType !== "mouse") return; const box = event.currentTarget.getBoundingClientRect(); setOffset({ x: (event.clientX - box.left - box.width / 2) * .16, y: (event.clientY - box.top - box.height / 2) * .16 }); }} onPointerLeave={() => setOffset({ x: 0, y: 0 })} whileTap={{ scale: .97 }}>{children}</motion.a>;
}
