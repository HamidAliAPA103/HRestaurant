import { z } from "zod";

export const loginSchema = z.object({
  email: z
    .string()
    .min(1, "Email daxil edin.")
    .email("Düzgün email ünvanı daxil edin."),
  password: z.string().min(1, "Şifrəni daxil edin."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
