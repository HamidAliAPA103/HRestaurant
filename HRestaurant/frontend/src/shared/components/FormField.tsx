import type { InputHTMLAttributes } from "react";

export function FormField({
  label,
  error,
  hint,
  ...props
}: InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  error?: string;
  hint?: string;
}) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-semibold text-[#3c3530]">
        {label}
      </span>
      <input
        className="h-12 w-full rounded-xl border border-[#dcd5cc] bg-white px-4 text-sm text-[#29231f] outline-none transition placeholder:text-[#aaa097] focus:border-[#e85d3f] focus:ring-3 focus:ring-[#e85d3f]/10"
        {...props}
      />
      {error ? (
        <span className="mt-1.5 block text-xs font-medium text-[#c94a33]">
          {error}
        </span>
      ) : hint ? (
        <span className="mt-1.5 block text-xs text-[#8a8078]">{hint}</span>
      ) : null}
    </label>
  );
}
