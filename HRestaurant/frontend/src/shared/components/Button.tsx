import {
  forwardRef,
  type ButtonHTMLAttributes,
} from "react";
import { LoaderCircle } from "lucide-react";
import { cn } from "@/shared/lib/utils";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "ghost" | "danger";
  size?: "sm" | "md" | "lg";
  loading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  function Button(
    {
      children,
      className,
      variant = "primary",
      size = "md",
      loading,
      disabled,
      ...props
    },
    ref,
  ) {
    return (
      <button
        ref={ref}
        className={cn(
          "inline-flex items-center justify-center gap-2 rounded-xl font-semibold transition-all duration-200",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#e85d3f] focus-visible:ring-offset-2",
          "disabled:pointer-events-none disabled:opacity-50",
          variant === "primary" &&
            "bg-[#e85d3f] text-white shadow-[0_8px_24px_rgba(232,93,63,.2)] hover:bg-[#d94e32] hover:-translate-y-0.5",
          variant === "secondary" &&
            "border border-[#ded8cf] bg-white text-[#29231f] hover:border-[#bdb4a9] hover:bg-[#faf8f4]",
          variant === "ghost" &&
            "text-[#6e645d] hover:bg-[#f0ece5] hover:text-[#29231f]",
          variant === "danger" &&
            "bg-[#fff0ed] text-[#c3422b] hover:bg-[#ffe2dc]",
          size === "sm" && "h-9 px-3 text-sm",
          size === "md" && "h-11 px-4 text-sm",
          size === "lg" && "h-13 px-5 text-base",
          className,
        )}
        disabled={disabled || loading}
        {...props}
      >
        {loading && <LoaderCircle className="h-4 w-4 animate-spin" />}
        {children}
      </button>
    );
  },
);
