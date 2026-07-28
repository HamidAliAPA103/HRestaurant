import { ArrowLeft, SearchX } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/shared/components/Button";

export function NotFoundPage() {
  return (
    <div className="grid min-h-screen place-items-center bg-[#f8f6f1] p-6 text-center">
      <div>
        <div className="mx-auto grid h-16 w-16 place-items-center rounded-3xl bg-[#eee9e2] text-[#665e57]">
          <SearchX className="h-7 w-7" />
        </div>
        <div className="mt-6 text-xs font-bold uppercase tracking-[0.2em] text-[#e85d3f]">
          404
        </div>
        <h1 className="mt-2 text-3xl font-bold tracking-tight">
          Səhifə tapılmadı
        </h1>
        <p className="mt-3 text-sm text-[#81776f]">
          Axtardığınız bölmə mövcud deyil və ya köçürülüb.
        </p>
        <Link to="/dashboard" className="mt-6 inline-block">
          <Button>
            <ArrowLeft className="h-4 w-4" />
            Dashboard-a qayıt
          </Button>
        </Link>
      </div>
    </div>
  );
}
