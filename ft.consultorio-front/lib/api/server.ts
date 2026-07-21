import "server-only"

import { ApiError, type ApiResponse, type ProblemDetails } from "./types"

/**
 * Cliente HTTP del lado servidor (BFF). Centraliza:
 *  - la URL base del Gateway,
 *  - la cabecera `Origin` que MsAuth exige (`Security:AllowedOrigins`), que un
 *    fetch server-side no envía por sí solo,
 *  - el desempaquetado de `ApiResponse<T>` y el mapeo de errores a {@link ApiError}.
 *
 * Nunca se importa desde componentes cliente (marcado `server-only`).
 */

function requireEnv(name: string): string {
  const value = process.env[name]
  if (!value) {
    throw new Error(`Falta la variable de entorno ${name}. Revisa .env.local`)
  }
  return value
}

const AUTH_BASE_URL = () => requireEnv("AUTH_API_BASE_URL").replace(/\/+$/, "")
const ALLOWED_ORIGIN = () => requireEnv("AUTH_ALLOWED_ORIGIN")

/**
 * URLs base por servicio, SIEMPRE a través del Gateway (YARP) — nunca al
 * microservicio directo. El Gateway enruta por prefijo (`/msauth`,
 * `/msmedicalrecords`) hacia el microservicio correspondiente.
 */
export type ApiService = "auth" | "medical-records"

function baseUrlFor(service: ApiService): string {
  if (service === "medical-records") {
    return requireEnv("MEDICAL_RECORDS_API_BASE_URL").replace(/\/+$/, "")
  }
  return AUTH_BASE_URL()
}

interface ServerFetchOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE"
  /** Servicio destino (elige la URL base del Gateway). Por defecto `auth`. */
  service?: ApiService
  /** Cuerpo JSON. Se serializa automáticamente. */
  body?: unknown
  /** Access token a enviar como `Authorization: Bearer`. */
  accessToken?: string
  /** Cabeceras adicionales. */
  headers?: Record<string, string>
  signal?: AbortSignal
}

/**
 * Ejecuta una llamada al backend y devuelve `data` ya desempaquetado.
 * Lanza {@link ApiError} en respuestas 4xx/5xx.
 */
export async function serverFetch<T>(
  path: string,
  options: ServerFetchOptions = {},
): Promise<T> {
  const { method = "GET", service = "auth", body, accessToken, headers, signal } = options

  const url = `${baseUrlFor(service)}${path.startsWith("/") ? path : `/${path}`}`

  const finalHeaders: Record<string, string> = {
    Accept: "application/json",
    Origin: ALLOWED_ORIGIN(),
    ...headers,
  }
  if (body !== undefined) {
    finalHeaders["Content-Type"] = "application/json"
  }
  if (accessToken) {
    finalHeaders["Authorization"] = `Bearer ${accessToken}`
  }

  let response: Response
  try {
    response = await fetch(url, {
      method,
      headers: finalHeaders,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      // El BFF nunca cachea llamadas de auth.
      cache: "no-store",
      signal,
    })
  } catch {
    throw new ApiError(0, {
      title: "No se pudo contactar el servidor",
      detail:
        "El servicio no está disponible. Intenta de nuevo en unos segundos.",
      status: 0,
    })
  }

  if (response.status === 204) {
    return undefined as T
  }

  const raw = await response.text()
  const payload = raw ? safeJsonParse(raw) : undefined

  if (!response.ok) {
    const problem: ProblemDetails =
      payload && typeof payload === "object"
        ? (payload as ProblemDetails)
        : { title: response.statusText, status: response.status }
    throw new ApiError(response.status, problem)
  }

  return (payload as ApiResponse<T>)?.data as T
}

/**
 * Igual que {@link serverFetch} pero devuelve también la metadata de paginación
 * del sobre `ApiResponse` (para listados paginados).
 */
export async function serverFetchPaged<T>(
  path: string,
  options: ServerFetchOptions = {},
): Promise<{ data: T; pagination: ApiResponse<T>["pagination"] }> {
  const { method = "GET", service = "auth", body, accessToken, headers, signal } =
    options

  const url = `${baseUrlFor(service)}${path.startsWith("/") ? path : `/${path}`}`

  const finalHeaders: Record<string, string> = {
    Accept: "application/json",
    Origin: ALLOWED_ORIGIN(),
    ...headers,
  }
  if (body !== undefined) finalHeaders["Content-Type"] = "application/json"
  if (accessToken) finalHeaders["Authorization"] = `Bearer ${accessToken}`

  let response: Response
  try {
    response = await fetch(url, {
      method,
      headers: finalHeaders,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      cache: "no-store",
      signal,
    })
  } catch {
    throw new ApiError(0, {
      title: "No se pudo contactar el servidor",
      detail: "El servicio no está disponible. Intenta de nuevo en unos segundos.",
      status: 0,
    })
  }

  const raw = await response.text()
  const payload = raw ? safeJsonParse(raw) : undefined

  if (!response.ok) {
    const problem: ProblemDetails =
      payload && typeof payload === "object"
        ? (payload as ProblemDetails)
        : { title: response.statusText, status: response.status }
    throw new ApiError(response.status, problem)
  }

  const envelope = payload as ApiResponse<T>
  return { data: envelope?.data as T, pagination: envelope?.pagination ?? null }
}

function safeJsonParse(raw: string): unknown {
  try {
    return JSON.parse(raw)
  } catch {
    return undefined
  }
}
