import { ApiError } from "@/lib/api/types"

/**
 * Estado compartido para formularios con `useActionState`.
 * `fieldErrors` se muestra por campo; `message` es el error/aviso general;
 * `values` repuebla el formulario tras un error.
 */
export interface FormState {
  ok: boolean
  message?: string
  fieldErrors?: Record<string, string>
  values?: Record<string, string>
}

export const emptyFormState: FormState = { ok: false }

/** Lee un campo del FormData como string recortado. */
export function str(data: FormData, key: string): string {
  return (data.get(key) ?? "").toString().trim()
}

/** Campo opcional: `null` cuando viene vacío (para no mandar "" al backend). */
export function strOrNull(data: FormData, key: string): string | null {
  const v = str(data, key)
  return v.length ? v : null
}

/** ¿Un checkbox marcado? */
export function bool(data: FormData, key: string): boolean {
  const v = data.get(key)
  return v === "on" || v === "true" || v === "1"
}

/** Convierte los errores por campo del backend (PascalCase) a camelCase. */
export function mapFieldErrors(error: ApiError): Record<string, string> {
  const out: Record<string, string> = {}
  for (const [key, messages] of Object.entries(error.fieldErrors)) {
    const camel = key.charAt(0).toLowerCase() + key.slice(1)
    if (messages?.length) out[camel] = messages[0]
  }
  return out
}

/** Traduce un error desconocido a `FormState`, preservando valores repoblables. */
export function toFormState(
  error: unknown,
  values?: Record<string, string>,
): FormState {
  if (error instanceof ApiError) {
    const fieldErrors = mapFieldErrors(error)
    return {
      ok: false,
      values,
      fieldErrors: Object.keys(fieldErrors).length ? fieldErrors : undefined,
      message: Object.keys(fieldErrors).length
        ? undefined
        : error.problem.detail || error.problem.title || error.message,
    }
  }
  return {
    ok: false,
    values,
    message: "Ocurrió un error inesperado. Intenta de nuevo.",
  }
}
