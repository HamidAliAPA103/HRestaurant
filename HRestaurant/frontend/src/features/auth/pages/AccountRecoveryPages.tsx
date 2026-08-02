import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { CheckCircle2, KeyRound, LoaderCircle, MailCheck } from "lucide-react";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Link, useSearchParams } from "react-router-dom";
import { z } from "zod";
import { authApi } from "@/api/authApi";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { getErrorMessage } from "@/shared/lib/utils";

const emailSchema = z.object({ email: z.string().email("Düzgün email daxil edin.") });
const resetSchema = z.object({ newPassword: z.string().min(8).regex(/[A-Z]/).regex(/[a-z]/).regex(/[0-9]/).regex(/[^a-zA-Z0-9]/), confirmPassword: z.string() }).refine((value) => value.newPassword === value.confirmPassword, { path: ["confirmPassword"], message: "Şifrələr uyğun deyil." });

function Shell({ title, description, children }: { title: string; description: string; children: React.ReactNode }) {
  return <main className="grid min-h-screen place-items-center bg-[#f6f2eb] p-4"><section className="w-full max-w-md rounded-3xl bg-white p-7 shadow-xl"><Link to="/login" className="text-sm font-bold text-[#d64f34]">← Girişə qayıt</Link><h1 className="mt-6 text-3xl font-bold">{title}</h1><p className="mt-2 text-sm leading-6 text-[#7d736b]">{description}</p><div className="mt-7">{children}</div></section></main>;
}

export function ForgotPasswordPage() {
  const form = useForm<z.infer<typeof emailSchema>>({ resolver: zodResolver(emailSchema), defaultValues: { email: "" } });
  const mutation = useMutation({ mutationFn: (value: { email: string }) => authApi.forgotPassword(value.email) });
  return <Shell title="Şifrəni yenilə" description="Hesab mövcuddursa, təhlükəsiz yeniləmə linki email ünvanına göndəriləcək.">{mutation.isSuccess ? <p className="flex items-center gap-2 rounded-xl bg-green-50 p-4 text-sm text-green-800"><CheckCircle2 className="h-5 w-5" />Sorğu qəbul edildi. Email qutusunu yoxlayın.</p> : <form className="space-y-4" onSubmit={form.handleSubmit((value) => mutation.mutate(value))}><FormField label="Email" type="email" autoComplete="email" error={form.formState.errors.email?.message} {...form.register("email")} />{mutation.isError && <p className="text-sm text-red-700">{getErrorMessage(mutation.error)}</p>}<Button type="submit" className="w-full" loading={mutation.isPending}><KeyRound className="h-4 w-4" />Link göndər</Button></form>}</Shell>;
}

export function ResetPasswordPage() {
  const [params] = useSearchParams(); const email = params.get("email") ?? ""; const token = params.get("token") ?? "";
  const form = useForm<z.infer<typeof resetSchema>>({ resolver: zodResolver(resetSchema), defaultValues: { newPassword: "", confirmPassword: "" } });
  const mutation = useMutation({ mutationFn: (value: z.infer<typeof resetSchema>) => authApi.resetPassword({ email, token, ...value }) });
  return <Shell title="Yeni şifrə" description="Yeni güclü şifrə təyin edin.">{!email || !token ? <p className="rounded-xl bg-red-50 p-4 text-sm text-red-700">Yeniləmə linki etibarsızdır.</p> : mutation.isSuccess ? <div className="space-y-4"><p className="rounded-xl bg-green-50 p-4 text-sm text-green-800">Şifrə uğurla yeniləndi.</p><Link to="/login" className="block rounded-xl bg-[#e85d3f] px-4 py-3 text-center font-bold text-white">Daxil ol</Link></div> : <form className="space-y-4" onSubmit={form.handleSubmit((value) => mutation.mutate(value))}><FormField label="Yeni şifrə" type="password" autoComplete="new-password" error={form.formState.errors.newPassword?.message} {...form.register("newPassword")} /><FormField label="Şifrəni təkrarla" type="password" autoComplete="new-password" error={form.formState.errors.confirmPassword?.message} {...form.register("confirmPassword")} />{mutation.isError && <p className="text-sm text-red-700">{getErrorMessage(mutation.error)}</p>}<Button type="submit" className="w-full" loading={mutation.isPending}>Şifrəni yenilə</Button></form>}</Shell>;
}

export function VerifyEmailPage() {
  const [params] = useSearchParams(); const userId = params.get("userId") ?? ""; const token = params.get("token") ?? "";
  const mutation = useMutation({ mutationFn: () => authApi.verifyEmail(userId, token) });
  useEffect(() => { if (userId && token && mutation.isIdle) mutation.mutate(); }, [mutation, token, userId]);
  return <Shell title="Email təsdiqi" description="Hesab məlumatları yoxlanılır."><div className="text-center">{mutation.isPending && <LoaderCircle className="mx-auto h-9 w-9 animate-spin text-[#e85d3f]" />}{mutation.isSuccess && <><MailCheck className="mx-auto h-10 w-10 text-green-600" /><p className="mt-3 text-green-800">Email ünvanı təsdiqləndi.</p></>}{(mutation.isError || !userId || !token) && <p className="rounded-xl bg-red-50 p-4 text-sm text-red-700">{mutation.isError ? getErrorMessage(mutation.error) : "Təsdiq linki etibarsızdır."}</p>}<Link to="/login" className="mt-5 inline-block font-bold text-[#d64f34]">Giriş səhifəsi</Link></div></Shell>;
}
