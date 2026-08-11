import { NextResponse, type NextRequest } from "next/server"

/**
 * Guarda de rutas del BFF (convención `proxy` de Next 16, ex-middleware).
 *
 * Además de proteger rutas por la presencia de la cookie de sesión (`ft_refresh`,
 * httpOnly), **refresca el access token de forma proactiva**: si expiró, redirige
 * a `/auth/refresh` (un Route Handler donde SÍ se pueden escribir cookies) antes
 * de renderizar la página. Esto es necesario porque el refresh token del backend
 * es rotativo con detección de reuso: refrescar durante el render (donde no se
 * pueden persistir cookies) perdería la rotación y cerraría la sesión. Ver
 * `lib/auth/session.ts` (`refreshSession`) y `app/auth/refresh/route.ts`.
 */

const AUTH_ROUTES = ["/login", "/register", "/verify-email"]
const REFRESH_PATH = "/auth/refresh"
const LOGOUT_PATH = "/logout"

/** Margen para renovar un poco antes de que el access token expire de verdad. */
const EXPIRY_SKEW_MS = 60_000

export function proxy(request: NextRequest) {
  const { pathname, search } = request.nextUrl
  const hasSession = Boolean(request.cookies.get("ft_refresh")?.value)
  const isAuthRoute = AUTH_ROUTES.some((route) => pathname.startsWith(route))
  // Las rutas que gestionan la propia sesión no deben re-dispararse a sí mismas.
  const isSessionRoute = pathname === REFRESH_PATH || pathname === LOGOUT_PATH

  // Usuario autenticado que entra a una pantalla de auth → al dashboard.
  if (isAuthRoute && hasSession) {
    return NextResponse.redirect(new URL("/", request.url))
  }

  // Ruta protegida sin sesión → al login, recordando el destino.
  if (!isAuthRoute && !isSessionRoute && !hasSession) {
    const loginUrl = new URL("/login", request.url)
    if (pathname !== "/") loginUrl.searchParams.set("next", pathname)
    return NextResponse.redirect(loginUrl)
  }

  // Ruta protegida con sesión pero access token expirado → renovar.
  if (
    hasSession &&
    !isAuthRoute &&
    !isSessionRoute &&
    accessExpired(request)
  ) {
    // En prefetch NO se refresca (rotaría el token en segundo plano y lo
    // perdería): se corta sin renderizar. La navegación real sí refresca.
    if (isPrefetch(request)) {
      return new NextResponse(null, { status: 401 })
    }
    // Solo navegaciones GET; las Server Actions (POST) se dejan pasar y, si el
    // token expiró, devolverán un error manejable en vez de perder el envío.
    if (request.method === "GET") {
      const refreshUrl = new URL(REFRESH_PATH, request.url)
      refreshUrl.searchParams.set("next", pathname + search)
      return NextResponse.redirect(refreshUrl)
    }
  }

  return NextResponse.next()
}

/** ¿El access token expiró (o le queda menos que el margen)? */
function accessExpired(request: NextRequest): boolean {
  const expiresAt = request.cookies.get("ft_access_exp")?.value
  if (!expiresAt) return true
  const t = new Date(expiresAt).getTime()
  return Number.isNaN(t) || t - Date.now() < EXPIRY_SKEW_MS
}

/** Detecta prefetches del router de Next / del navegador. */
function isPrefetch(request: NextRequest): boolean {
  return (
    request.headers.get("next-router-prefetch") === "1" ||
    request.headers.get("purpose") === "prefetch" ||
    (request.headers.get("sec-purpose")?.includes("prefetch") ?? false)
  )
}

export const config = {
  // Excluye assets estáticos y recursos internos de Next.
  matcher: ["/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)"],
}
