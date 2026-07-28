export type AppRole =
  | "Admin"
  | "Owner"
  | "Manager"
  | "Cashier"
  | "Waiter"
  | "Chef"
  | "Host"
  | "Customer";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  restaurantId: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  tokenType: "Bearer";
}

export interface AuthUser {
  id: string;
  email: string;
  restaurantId: string;
  roles: AppRole[];
}

export interface JwtPayload {
  user_id: string;
  email: string;
  restaurant_id: string;
  role: AppRole | AppRole[];
  exp: number;
}
