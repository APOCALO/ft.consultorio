/**
 * Estado que consumen los formularios de auth con `useActionState`.
 * Vive fuera de `actions.ts` porque un módulo `"use server"` solo puede
 * exportar funciones async (Next 16).
 *
 * `fieldErrors` se muestra por campo; `message` es el error/aviso general.
 * `values` repuebla el formulario tras un error (excepto contraseñas).
 */
export interface FormState {
  ok: boolean
  message?: string
  fieldErrors?: Record<string, string>
  values?: Record<string, string>
}

export const emptyFormState: FormState = { ok: false }
