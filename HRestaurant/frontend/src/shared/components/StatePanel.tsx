import {
  AlertTriangle,
  Inbox,
  LoaderCircle,
  RefreshCw,
} from "lucide-react";
import { Button } from "@/shared/components/Button";

export function LoadingState({ label = "Məlumatlar yüklənir" }) {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center rounded-2xl border border-[#e6e0d8] bg-white p-8 text-center">
      <LoaderCircle className="h-7 w-7 animate-spin text-[#e85d3f]" />
      <p className="mt-3 text-sm font-medium text-[#726960]">{label}</p>
    </div>
  );
}

export function EmptyState({
  title = "Hələ məlumat yoxdur",
  description = "Yeni məlumat əlavə etdikdə burada görünəcək.",
  action,
}: {
  title?: string;
  description?: string;
  action?: React.ReactNode;
}) {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center rounded-2xl border border-dashed border-[#d9d1c7] bg-[#fbfaf7] p-8 text-center">
      <div className="grid h-12 w-12 place-items-center rounded-2xl bg-[#eee9e1] text-[#746a61]">
        <Inbox className="h-5 w-5" />
      </div>
      <h3 className="mt-4 font-bold text-[#29231f]">{title}</h3>
      <p className="mt-1 max-w-sm text-sm text-[#7b7169]">{description}</p>
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}

export function ErrorState({
  message = "Məlumatları yükləmək mümkün olmadı.",
  onRetry,
}: {
  message?: string;
  onRetry?: () => void;
}) {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center rounded-2xl border border-[#f0d2cb] bg-[#fff8f6] p-8 text-center">
      <div className="grid h-12 w-12 place-items-center rounded-2xl bg-[#ffe7e1] text-[#c94e37]">
        <AlertTriangle className="h-5 w-5" />
      </div>
      <h3 className="mt-4 font-bold text-[#29231f]">Bağlantı xətası</h3>
      <p className="mt-1 max-w-md text-sm text-[#7b625c]">{message}</p>
      {onRetry && (
        <Button
          variant="secondary"
          size="sm"
          className="mt-4"
          onClick={onRetry}
        >
          <RefreshCw className="h-4 w-4" />
          Yenidən yoxla
        </Button>
      )}
    </div>
  );
}
