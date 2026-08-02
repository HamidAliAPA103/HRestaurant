export type AppRole =
  | "SuperAdmin"
  | "RestaurantOwner"
  | "Manager"
  | "Cashier"
  | "Waiter"
  | "Chef";

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
  fullName: string;
  email: string;
  restaurantId: string;
  branchId?: string;
  roles: AppRole[];
  permissions: string[];
}

export interface JwtPayload {
  user_id: string;
  email: string;
  restaurant_id: string;
  branch_id?: string;
  full_name: string;
  role: AppRole | AppRole[];
  permission?: string | string[];
  exp: number;
}
