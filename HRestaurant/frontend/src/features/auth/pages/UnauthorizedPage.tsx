import { ArrowLeft, ShieldX } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/shared/components/Button";

export function UnauthorizedPage() {
  return (
    <div className="grid min-h-[70vh] place-items-center p-6 text-center">
      <div>
        <div className="mx-auto grid h-16 w-16 place-items-center rounded-3xl bg-[#ffe9e4] text-[#c64c35]">
          <ShieldX className="h-7 w-7" />
        </div>
        <h1 className="mt-6 text-3xl font-bold tracking-tight text-[#29231f]">
          Bu bölməyə giriş yoxdur
        </h1>
        <p className="mx-auto mt-3 max-w-md text-sm leading-6 text-[#7c726a]">
          Hesabınızın rolu bu səhifəni açmağa icazə vermir. Səlahiyyət üçün
          restoran administratoruna müraciət edin.
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
