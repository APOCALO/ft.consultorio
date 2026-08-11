import { redirect } from "next/navigation"

import { refreshSession } from "@/lib/auth/session"

/**
 * Renovación silenciosa de la sesión (BFF). Es un Route Handler porque —junto a
 * las Server Actions— es el único contexto donde Next permite **escribir**
 * cookies; el refresh token del backend es rotativo con detección de reuso, así
 * que la rotación debe persistirse aquí (no en el render).
 *
 * El `proxy` redirige aquí cuando detecta el access token expirado en una
 * navegación real, con `?next=<ruta>` para volver al destino original.
 * Si el refresco falla, la sesión ya quedó limpia → se manda a /login.
 */
export async function GET(request: Request) {
  const next = safeNext(new URL(request.url).searchParams.get("next"))

  const ok = await refreshSession()

  // `redirect` lanza NEXT_REDIRECT: fuera de cualquier try para no atraparlo.
  redirect(ok ? next : "/login")
}

/** Evita open-redirects: solo rutas internas absolutas. */
function safeNext(next: string | null): string {
  if (next && next.startsWith("/") && !next.startsWith("//")) return next
  return "/"
}
