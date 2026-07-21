import { redirect } from "next/navigation"

import { endSession } from "@/lib/auth/session"

/**
 * Cierre de sesión vía Route Handler (GET), el único contexto — junto a las
 * Server Actions — donde Next permite **escribir** cookies. Se usa como destino
 * de redirección cuando el render detecta una sesión inválida (401): ahí no se
 * pueden borrar las cookies, así que se redirige aquí para limpiarlas de verdad
 * y romper el bucle `/login` ↔ `/` que provocaría una cookie de sesión obsoleta.
 */
export async function GET() {
  await endSession()
  redirect("/login")
}
