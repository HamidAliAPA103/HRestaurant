import { CrunchSection } from "../components/CrunchSection";
import { CtaSection } from "../components/CtaSection";
import { DressingSection } from "../components/DressingSection";
import { Footer } from "../components/Footer";
import { Header } from "../components/Header";
import { HeroSection } from "../components/HeroSection";
import { MenuSection } from "../components/MenuSection";

export function VerdantTablePage() {
  return <div className="min-h-screen overflow-x-hidden bg-[#fcfbf6] text-[#173d2b]"><Header /><main><HeroSection /><MenuSection /><CrunchSection /><DressingSection /><CtaSection /></main><Footer /></div>;
}
