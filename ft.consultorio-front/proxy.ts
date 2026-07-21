import { NextResponse, type NextRequest } from "next/server"

/**
 * Guarda de rutas del BFF (convención `proxy` de Next 16, ex-middleware).
 * Sólo comprueba la presencia de la cookie de sesión (`ft_refresh`, httpOnly);
 * la validez real del token la verifica el backend en cada llamada. La
 * renovación silenciosa vive en `lib/auth/session.ts`.
 */

const AUTH_ROUTES = ["/login", "/register", "/verify-email"]

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl
  const hasSession = Boolean(request.cookies.get("ft_refresh")?.value)
  const isAuthRoute = AUTH_ROUTES.some((route) => pathname.startsWith(route))

  // Usuario autenticado que entra a una pantalla de auth → al dashboard.
  if (isAuthRoute && hasSession) {
    return NextResponse.redirect(new URL("/", request.url))
  }

  // Ruta protegida sin sesión → al login, recordando el destino.
  if (!isAuthRoute && !hasSession) {
    const loginUrl = new URL("/login", request.url)
    if (pathname !== "/") loginUrl.searchParams.set("next", pathname)
    return NextResponse.redirect(loginUrl)
  }

  return NextResponse.next()
}

export const config = {
  // Excluye assets estáticos y recursos internos de Next.
  matcher: ["/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)"],
}
