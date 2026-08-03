import { describe, expect, it } from "vitest";
import { customerInformationSchema, reservationLookupSchema } from "./public-reservation-schema";

describe("customerInformationSchema", () => {
  it("accepts valid customer information", () => {
    const result = customerInformationSchema.safeParse({
      fullName: "Aydan Şərifova",
      phone: "+994501234567",
      email: "aydan@example.com",
      specialNotes: "Pəncərə kənarı",
      termsAccepted: true,
    });

    expect(result.success).toBe(true);
  });

  it("rejects invalid email and unaccepted terms", () => {
    const result = customerInformationSchema.safeParse({
      fullName: "Aydan Şərifova",
      phone: "+994501234567",
      email: "invalid-email",
      specialNotes: "",
      termsAccepted: false,
    });

    expect(result.success).toBe(false);
    expect(result.error?.issues.map((issue) => issue.path[0])).toEqual(
      expect.arrayContaining(["email", "termsAccepted"]),
    );
  });
});

describe("reservationLookupSchema", () => {
  it("accepts confirmation code with phone and normalizes the code", () => {
    const result = reservationLookupSchema.safeParse({ confirmationCode: "rsv-8f3k2m", phone: "+994553898484", trackingToken: "" });
    expect(result.success).toBe(true);
    if (result.success) expect(result.data.confirmationCode).toBe("RSV-8F3K2M");
  });

  it("rejects empty and competing lookup methods", () => {
    expect(reservationLookupSchema.safeParse({ confirmationCode: "", phone: "", trackingToken: "" }).success).toBe(false);
    expect(reservationLookupSchema.safeParse({ confirmationCode: "RSV-8F3K2M", phone: "+994553898484", trackingToken: "x".repeat(64) }).success).toBe(false);
  });
});
