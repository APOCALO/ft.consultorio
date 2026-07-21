/** Rol devuelto por MsAuth dentro de la sesión de usuario. */
export interface Role {
  id: string
  name: string
}

/** Usuario autenticado tal como lo expone `UserSessionResponseDTO`. */
export interface SessionUser {
  id: string
  userName: string
  userEmail: string
  firstName: string
  lastName: string
  fullName: string
  avatarUrl?: string | null
  isLocal: boolean
  isActive: boolean
  lastLoginAt?: string | null
  isFirstLogin: boolean
  roles: Role[]
}

/** Respuesta de login / verificación de OTP (`AuthTokenResponseDTO`). */
export interface AuthTokens {
  accessToken: string
  tokenType: string
  expiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  isFirstLogin: boolean
  user: SessionUser
}

/** Respuesta de registro pendiente de verificación (`RegisterLocalUserPendingResponseDTO`). */
export interface RegisterPending {
  userId: string
  email: string
  requiresEmailVerification: boolean
  otpLength: number
  otpExpiresInMinutes: number
  resendCooldownSeconds: number
}

/** Respuesta de reenvío de OTP (`ResendEmailOtpResponseDTO`). */
export interface ResendOtpResult {
  email: string
  otpLength: number
  otpExpiresInMinutes: number
  resendCooldownSeconds: number
}
