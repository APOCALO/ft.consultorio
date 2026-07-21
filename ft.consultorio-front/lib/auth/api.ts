import "server-only"

import { serverFetch } from "@/lib/api/server"
import type {
  AuthTokens,
  RegisterPending,
  ResendOtpResult,
} from "./types"

/**
 * Llamadas al microservicio de autenticación (MsAuth) vía Gateway.
 * Cada función mapea 1:1 a un endpoint de `AuthController`.
 * Se usan exclusivamente desde Server Actions / componentes servidor.
 */

const BASE = "/auth"

export interface RegisterInput {
  email: string
  password: string
  firstName: string
  lastName: string
  /** Formato ISO `YYYY-MM-DD` (DateOnly en el backend). */
  birthDate: string
  userName?: string
}

export function registerLocalUser(input: RegisterInput): Promise<RegisterPending> {
  return serverFetch<RegisterPending>(`${BASE}/register`, {
    method: "POST",
    body: input,
  })
}

export function verifyEmailOtp(email: string, otp: string): Promise<AuthTokens> {
  return serverFetch<AuthTokens>(`${BASE}/verify-email-otp`, {
    method: "POST",
    body: { email, otp },
  })
}

export function resendEmailOtp(email: string): Promise<ResendOtpResult> {
  return serverFetch<ResendOtpResult>(`${BASE}/resend-email-otp`, {
    method: "POST",
    body: { email },
  })
}

export function loginLocalUser(email: string, password: string): Promise<AuthTokens> {
  return serverFetch<AuthTokens>(`${BASE}/login`, {
    method: "POST",
    body: { email, password },
  })
}

export function refreshTokens(refreshToken: string): Promise<AuthTokens> {
  return serverFetch<AuthTokens>(`${BASE}/refresh`, {
    method: "POST",
    body: { refreshToken },
  })
}

export function revokeRefreshToken(refreshToken: string): Promise<boolean> {
  return serverFetch<boolean>(`${BASE}/logout`, {
    method: "POST",
    body: { refreshToken },
  })
}
