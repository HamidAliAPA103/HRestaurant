import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import {
  ArrowRight,
  Eye,
  EyeOff,
  LockKeyhole,
  Sparkles,
  Utensils,
} from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Navigate,
  Link,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { login } from "@/features/auth/api/auth-api";
import {
  loginSchema,
  type LoginFormValues,
} from "@/features/auth/schemas/login-schema";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { getErrorMessage } from "@/shared/lib/utils";

export function LoginPage() {
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const accessToken = useAuthStore((state) => state.accessToken);
  const setSession = useAuthStore((state) => state.setSession);
  const from =
    (location.state as { from?: string } | null)?.from ?? "/dashboard";

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (response) => {
      if (!response.success || !response.data) {
        throw new Error(response.message);
      }
      setSession(response.data);
      navigate(from, { replace: true });
    },
  });

  if (accessToken) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <main className="min-h-screen bg-[#f6f2eb] p-3 sm:p-5">
      <div className="mx-auto grid min-h-[calc(100vh-1.5rem)] max-w-[1500px] overflow-hidden rounded-[28px] bg-white shadow-[0_30px_100px_rgba(44,34,26,.12)] sm:min-h-[calc(100vh-2.5rem)] lg:grid-cols-[1.08fr_.92fr]">
        <section className="relative hidden overflow-hidden bg-[#201b18] p-10 text-white lg:flex lg:flex-col xl:p-14">
          <div className="absolute -right-28 -top-32 h-96 w-96 rounded-full bg-[#e85d3f]/20 blur-3xl" />
          <div className="absolute -bottom-24 left-12 h-80 w-80 rounded-full bg-[#e9a64a]/10 blur-3xl" />

          <div className="relative z-10 flex items-center gap-3">
            <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#e85d3f]">
              <Utensils className="h-5 w-5" />
            </div>
            <div>
              <div className="text-lg font-bold tracking-tight">
                HRestaurant
              </div>
              <div className="text-xs text-white/45">Operations OS</div>
            </div>
          </div>

          <div className="relative z-10 my-auto max-w-xl py-16">
            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-3 py-1.5 text-xs font-semibold text-white/70">
              <Sparkles className="h-3.5 w-3.5 text-[#f0aa51]" />
              Restoranınızın bütün ritmi bir mərkəzdə
            </div>
            <h1 className="text-5xl font-bold leading-[1.05] tracking-[-0.055em] xl:text-6xl">
              Daha rahat servis.
              <span className="block text-[#e85d3f]">Daha ağıllı idarəetmə.</span>
            </h1>
            <p className="mt-6 max-w-lg text-base leading-7 text-white/55">
              Masalardan mətbəxə, rezervasiyadan hesabatlara qədər bütün
              əməliyyatlarınız real vaxtda sizinlədir.
            </p>

            <div className="mt-10 grid max-w-lg grid-cols-2 gap-3 text-sm text-white/60">
              <div className="rounded-2xl border border-white/8 bg-white/5 p-4">Canlı sifariş və mətbəx axını</div>
              <div className="rounded-2xl border border-white/8 bg-white/5 p-4">Rezervasiya və hesabat idarəetməsi</div>
            </div>
          </div>

          <p className="relative z-10 text-xs text-white/30">
            © 2026 HRestaurant. İşinizi sadələşdiririk.
          </p>
        </section>

        <section className="flex items-center justify-center px-6 py-12 sm:px-12 lg:px-16 xl:px-24">
          <div className="w-full max-w-md">
            <div className="mb-10 flex items-center gap-3 lg:hidden">
              <div className="grid h-10 w-10 place-items-center rounded-xl bg-[#e85d3f] text-white">
                <Utensils className="h-5 w-5" />
              </div>
              <span className="text-lg font-bold">HRestaurant</span>
            </div>

            <div className="grid h-12 w-12 place-items-center rounded-2xl bg-[#f3ede5] text-[#e85d3f]">
              <LockKeyhole className="h-5 w-5" />
            </div>
            <h2 className="mt-6 text-4xl font-bold tracking-[-0.045em] text-[#211c19]">
              Yenidən xoş gəldiniz
            </h2>
            <p className="mt-3 text-sm leading-6 text-[#7d736b]">
              İdarəetmə panelinə davam etmək üçün hesabınıza daxil olun.
            </p>

            <form
              className="mt-8 space-y-5"
              onSubmit={handleSubmit((values) => mutation.mutate(values))}
            >
              <FormField
                label="Email ünvanı"
                type="email"
                autoComplete="email"
                placeholder="name@restaurant.az"
                error={errors.email?.message}
                {...register("email")}
              />

              <div>
                <div className="mb-2 flex items-center justify-between"><label className="text-sm font-semibold text-[#3c3530]">Şifrə</label><Link to="/forgot-password" className="text-xs font-bold text-[#d64f34]">Şifrəni unutmusunuz?</Link></div>
                <div className="relative">
                  <input
                    type={showPassword ? "text" : "password"}
                    autoComplete="current-password"
                    placeholder="Şifrənizi daxil edin"
                    className="h-12 w-full rounded-xl border border-[#dcd5cc] bg-white px-4 pr-12 text-sm outline-none transition placeholder:text-[#aaa097] focus:border-[#e85d3f] focus:ring-3 focus:ring-[#e85d3f]/10"
                    {...register("password")}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((value) => !value)}
                    className="absolute inset-y-0 right-0 grid w-12 place-items-center text-[#8a8179] hover:text-[#3a342f]"
                    aria-label={showPassword ? "Şifrəni gizlət" : "Şifrəni göstər"}
                  >
                    {showPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </button>
                </div>
                {errors.password && (
                  <p className="mt-1.5 text-xs font-medium text-[#c94a33]">
                    {errors.password.message}
                  </p>
                )}
              </div>

              {mutation.isError && (
                <div className="rounded-xl border border-[#f1cec6] bg-[#fff6f4] px-4 py-3 text-sm text-[#ad402d]">
                  {getErrorMessage(mutation.error)}
                </div>
              )}

              <Button
                type="submit"
                size="lg"
                className="w-full"
                loading={mutation.isPending}
              >
                Daxil ol
                {!mutation.isPending && <ArrowRight className="h-4 w-4" />}
              </Button>
            </form>

            <p className="mt-5 text-center text-sm text-[#7d736b]">Yeni və sahibsiz restoranı qoşursunuz? <Link to="/register" className="font-bold text-[#d64f34]">İlk owner hesabını yaradın</Link></p>

            <div className="mt-8 flex items-center justify-center gap-2 text-xs text-[#928980]">
              <span className="h-1.5 w-1.5 rounded-full bg-[#64a779]" />
              Sistem təhlükəsiz JWT bağlantısı ilə qorunur
            </div>
          </div>
        </section>
      </div>
    </main>
  );
}
