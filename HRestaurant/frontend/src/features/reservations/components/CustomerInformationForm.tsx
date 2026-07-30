import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft, ArrowRight } from "lucide-react";
import { useForm } from "react-hook-form";
import {
  customerInformationSchema,
  type CustomerInformationFormValue,
} from "@/schemas/public-reservation-schema";
import type { CustomerInformation } from "@/types/public";

interface CustomerInformationFormProps {
  initialValue: CustomerInformation | null;
  onBack: () => void;
  onSubmit: (value: CustomerInformationFormValue) => void;
}

export function CustomerInformationForm({
  initialValue,
  onBack,
  onSubmit,
}: CustomerInformationFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CustomerInformationFormValue>({
    resolver: zodResolver(customerInformationSchema),
    defaultValues: initialValue ?? {
      fullName: "",
      phone: "",
      email: "",
      specialNotes: "",
      termsAccepted: false,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div className="grid gap-5 sm:grid-cols-2">
        <FormField
          label="Ad və soyad"
          error={errors.fullName?.message}
        >
          <input
            {...register("fullName")}
            autoComplete="name"
            className={inputClass}
            placeholder="Aydan Şərifova"
          />
        </FormField>
        <FormField label="Telefon" error={errors.phone?.message}>
          <input
            {...register("phone")}
            type="tel"
            autoComplete="tel"
            className={inputClass}
            placeholder="+994 50 123 45 67"
          />
        </FormField>
      </div>
      <FormField label="E-poçt (istəyə bağlı)" error={errors.email?.message}>
        <input
          {...register("email")}
          type="email"
          autoComplete="email"
          className={inputClass}
          placeholder="aydan@example.com"
        />
      </FormField>
      <FormField
        label="Xüsusi qeyd (istəyə bağlı)"
        error={errors.specialNotes?.message}
      >
        <textarea
          {...register("specialNotes")}
          rows={4}
          className={`${inputClass} resize-none`}
          placeholder="Pəncərə kənarında masa, allergiya məlumatı və s."
        />
      </FormField>
      <label className="flex cursor-pointer items-start gap-3 rounded-2xl bg-stone-50 p-4">
        <input
          {...register("termsAccepted")}
          type="checkbox"
          className="mt-1 h-4 w-4 accent-[#b5422d]"
        />
        <span className="text-sm leading-6 text-[#5f554d]">
          Rezervasiya şərtlərini və şəxsi məlumatların bu rezervasiya üçün
          işlənməsini qəbul edirəm.
          {errors.termsAccepted && (
            <span className="block font-semibold text-red-700">
              {errors.termsAccepted.message}
            </span>
          )}
        </span>
      </label>
      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-between">
        <button
          type="button"
          onClick={onBack}
          className="inline-flex items-center justify-center gap-2 rounded-full border border-[#d6ccc1] px-5 py-3 font-bold transition hover:bg-stone-50"
        >
          <ArrowLeft className="h-4 w-4" />
          Geri
        </button>
        <button
          type="submit"
          className="inline-flex items-center justify-center gap-2 rounded-full bg-[#b5422d] px-6 py-3 font-bold text-white transition hover:bg-[#983622]"
        >
          Xülasəyə keç
          <ArrowRight className="h-4 w-4" />
        </button>
      </div>
    </form>
  );
}

const inputClass =
  "w-full rounded-2xl border border-[#d9d0c6] bg-white px-4 py-3 outline-none transition focus:border-[#b5422d] focus:ring-4 focus:ring-[#b5422d]/10";

interface FormFieldProps {
  label: string;
  error?: string;
  children: React.ReactNode;
}

function FormField({ label, error, children }: FormFieldProps) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-bold text-[#4d443d]">
        {label}
      </span>
      {children}
      {error && (
        <span className="mt-1.5 block text-sm font-semibold text-red-700">
          {error}
        </span>
      )}
    </label>
  );
}
