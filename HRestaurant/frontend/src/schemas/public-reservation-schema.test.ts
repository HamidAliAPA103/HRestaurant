import { describe, expect, it } from "vitest";
import { customerInformationSchema } from "./public-reservation-schema";

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
