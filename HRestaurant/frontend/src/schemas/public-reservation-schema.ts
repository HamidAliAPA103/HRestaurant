import { z } from "zod";

export const customerInformationSchema = z.object({
  fullName: z
    .string()
    .trim()
    .min(2, "Ad və soyad ən azı 2 simvol olmalıdır.")
    .max(100, "Ad və soyad 100 simvoldan uzun ola bilməz."),
  phone: z
    .string()
    .trim()
    .regex(/^\+?[0-9 ()-]{7,20}$/, "Telefon formatı düzgün deyil."),
  email: z
    .string()
    .trim()
    .email("E-poçt formatı düzgün deyil.")
    .max(254)
    .optional()
    .or(z.literal("")),
  specialNotes: z
    .string()
    .trim()
    .max(500, "Xüsusi qeyd 500 simvoldan uzun ola bilməz.")
    .optional(),
  termsAccepted: z
    .boolean()
    .refine((value) => value, "Şərtləri qəbul etməlisiniz."),
});

export const reservationLookupSchema = z
  .object({
    confirmationCode: z.string().trim().optional(),
    phone: z.string().trim().optional(),
    trackingToken: z.string().trim().optional(),
  })
  .superRefine((value, context) => {
    const hasToken = Boolean(value.trackingToken);
    const hasCodeAndPhone = Boolean(
      value.confirmationCode && value.phone,
    );

    if (hasToken === hasCodeAndPhone) {
      context.addIssue({
        code: "custom",
        message:
          "Tracking token və ya təsdiq kodu ilə telefonu daxil edin.",
        path: ["confirmationCode"],
      });
    }
  });

export type CustomerInformationFormValue = z.infer<
  typeof customerInformationSchema
>;
export type ReservationLookupFormValue = z.infer<
  typeof reservationLookupSchema
>;
