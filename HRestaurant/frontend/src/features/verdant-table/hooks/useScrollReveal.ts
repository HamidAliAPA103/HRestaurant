import { useEffect, useRef, useState } from "react";

export function useScrollReveal<T extends HTMLElement>(delay = 0) {
  const ref = useRef<T>(null);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const element = ref.current;
    if (!element) return;
    const observer = new IntersectionObserver(([entry]) => {
      if (!entry.isIntersecting) return;
      window.setTimeout(() => setVisible(true), delay);
      observer.unobserve(entry.target);
    }, { threshold: 0.15 });
    observer.observe(element);
    return () => observer.disconnect();
  }, [delay]);

  return { ref, visible };
}
