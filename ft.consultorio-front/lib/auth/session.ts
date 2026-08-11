import "server-only"

import { cache } from "react"
import { cookies } from "next/headers"

import { refreshTokens, revokeRefreshToken } from "./api"
import type { AuthTokens, SessionUser } from "./types"

/**
 * Gestión de sesión del BFF. Los tokens viven en cookies **httpOnly** (no
 * accesibles desde JS del navegador → resistentes a XSS). El usuario se guarda
 * en una cookie httpOnly aparte para que los Server Components puedan pintarlo
 * sin volver a llamar al backend.
 */

const ACCESS_COOKIE = "ft_access"
const REFRESH_COOKIE = "ft_refresh"
const USER_COOKIE = "ft_user"
const EXPIRES_COOKIE = "ft_access_exp"

const isProd = process.env.NODE_ENV === "production"

function baseCookie(expires: Date) {
  return {
    httpOnly: true,
    secure: isProd,
    sameSite: "lax" as const,
    path: "/",
    expires,
  }
}

/**
 * Next.js sólo permite **escribir** cookies en Server Actions o Route Handlers,
 * no durante el render de un Server Component. La renovación silenciosa puede
 * dispararse en cualquiera de los dos contextos, así que las escrituras se hacen
 * "best-effort": si estamos en un render, la escritura se ignora y el token
 * renovado se usa igual para esta petición (se persistirá en la próxima acción).
 */
function safeCookieWrite(write: () => void): void {
  try {
    write()
  } catch {
    // Contexto de render: no se pueden modificar cookies. Se ignora.
  }
}

/** Persiste una sesión recién emitida por el backend. */
export async function setSession(tokens: AuthTokens): Promise<void> {
  const store = await cookies()
  const refreshExpires = new Date(tokens.refreshTokenExpiresAt)

  // El access token se guarda con la vida del refresh: aunque el access expire,
  // conservamos la cookie para poder renovarlo silenciosamente.
  safeCookieWrite(() => {
    store.set(ACCESS_COOKIE, tokens.accessToken, baseCookie(refreshExpires))
    store.set(REFRESH_COOKIE, tokens.refreshToken, baseCookie(refreshExpires))
    store.set(EXPIRES_COOKIE, tokens.expiresAt, baseCookie(refreshExpires))
    store.set(USER_COOKIE, JSON.stringify(tokens.user), baseCookie(refreshExpires))
  })
}

/** Borra todas las cookies de sesión. */
export async function clearSession(): Promise<void> {
  const store = await cookies()
  safeCookieWrite(() => {
    for (const name of [ACCESS_COOKIE, REFRESH_COOKIE, USER_COOKIE, EXPIRES_COOKIE]) {
      store.delete(name)
    }
  })
}

/** Usuario autenticado actual (de la cookie), o `null`. */
export async function getCurrentUser(): Promise<SessionUser | null> {
  const store = await cookies()
  const raw = store.get(USER_COOKIE)?.value
  if (!raw) return null
  try {
    return JSON.parse(raw) as SessionUser
  } catch {
    return null
  }
}

/** ¿Hay una sesión (refresh token presente)? */
export async function hasSession(): Promise<boolean> {
  const store = await cookies()
  return Boolean(store.get(REFRESH_COOKIE)?.value)
}

/**
 * Devuelve un access token válido, renovándolo con el refresh token si expiró.
 * Pensado para llamadas a APIs protegidas desde Server Components/Actions.
 * Devuelve `null` si no hay sesión o la renovación falla (sesión cerrada).
 */
export const getValidAccessToken = cache(
  async (): Promise<string | null> => {
    const store = await cookies()
    const access = store.get(ACCESS_COOKIE)?.value
    const expiresAt = store.get(EXPIRES_COOKIE)?.value

    const stillValid =
      access && expiresAt && new Date(expiresAt).getTime() - Date.now() > 30_000
    return stillValid ? access! : null
  },
)

/**
 * Renueva la sesión usando el refresh token. **Solo debe llamarse desde un Route
 * Handler o Server Action** (contextos donde Next permite escribir cookies): el
 * refresh token del backend es rotativo con detección de reuso, así que la
 * rotación DEBE persistirse o la siguiente petición dispara el reuso y cierra
 * todas las sesiones. Por eso el refresco NO se hace durante el render
 * (`getValidAccessToken` es de solo lectura) sino en `app/auth/refresh/route.ts`,
 * al que el `proxy` redirige de forma proactiva cuando el access token expiró.
 *
 * Devuelve `true` si renovó y persistió; `false` si no había sesión o el backend
 * la rechazó (en cuyo caso limpia las cookies).
 */
export async function refreshSession(): Promise<boolean> {
  const store = await cookies()
  const refresh = store.get(REFRESH_COOKIE)?.value
  if (!refresh) return false

  try {
    const renewed = await refreshTokens(refresh)
    await setSession(renewed)
    return true
  } catch {
    await clearSession()
    return false
  }
}

/** Cierra la sesión: revoca el refresh token en el backend y limpia cookies. */
export async function endSession(): Promise<void> {
  const store = await cookies()
  const refresh = store.get(REFRESH_COOKIE)?.value
  if (refresh) {
    try {
      await revokeRefreshToken(refresh)
    } catch {
      // Best-effort: aunque el backend falle, limpiamos la sesión local.
    }
  }
  await clearSession()
}
