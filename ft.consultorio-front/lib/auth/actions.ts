"use server"

import { redirect } from "next/navigation"

import { ApiError } from "@/lib/api/types"
import {
  loginLocalUser,
  registerLocalUser,
  resendEmailOtp,
  verifyEmailOtp,
} from "./api"
import type { FormState } from "./form-state"
import { endSession, setSession } from "./session"

// --- Helpers -----------------------------------------------------------------

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function str(data: FormData, key: string): string {
  return (data.get(key) ?? "").toString().trim()
}

/** Convierte los errores por campo del backend (PascalCase) a camelCase. */
function mapFieldErrors(error: ApiError): Record<string, string> {
  const out: Record<string, string> = {}
  for (const [key, messages] of Object.entries(error.fieldErrors)) {
    const camel = key.charAt(0).toLowerCase() + key.slice(1)
    if (messages?.length) out[camel] = messages[0]
  }
  return out
}

/** Traduce un `ApiError` a `FormState`, preservando valores repoblables. */
function toFormState(error: unknown, values: Record<string, string>): FormState {
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

function validatePassword(password: string): string | undefined {
  if (password.length < 8) return "Debe tener al menos 8 caracteres."
  if (!/[A-Z]/.test(password)) return "Debe incluir una mayúscula."
  if (!/[a-z]/.test(password)) return "Debe incluir una minúscula."
  if (!/[0-9]/.test(password)) return "Debe incluir un número."
  if (!/[^a-zA-Z0-9]/.test(password)) return "Debe incluir un símbolo."
  return undefined
}

// --- Actions -----------------------------------------------------------------

export async function loginAction(
  _prev: FormState,
  formData: FormData,
): Promise<FormState> {
  const email = str(formData, "email")
  const password = formData.get("password")?.toString() ?? ""
  const values = { email }

  const fieldErrors: Record<string, string> = {}
  if (!EMAIL_RE.test(email)) fieldErrors.email = "Correo inválido."
  if (!password) fieldErrors.password = "Ingresa tu contraseña."
  if (Object.keys(fieldErrors).length) return { ok: false, values, fieldErrors }

  try {
    const tokens = await loginLocalUser(email, password)
    await setSession(tokens)
  } catch (error) {
    return toFormState(error, values)
  }

  redirect("/")
}

export async function registerAction(
  _prev: FormState,
  formData: FormData,
): Promise<FormState> {
  const email = str(formData, "email")
  const firstName = str(formData, "firstName")
  const lastName = str(formData, "lastName")
  const birthDate = str(formData, "birthDate")
  const password = formData.get("password")?.toString() ?? ""
  const values = { email, firstName, lastName, birthDate }

  const fieldErrors: Record<string, string> = {}
  if (!EMAIL_RE.test(email)) fieldErrors.email = "Correo inválido."
  if (!firstName) fieldErrors.firstName = "Ingresa tu nombre."
  if (!lastName) fieldErrors.lastName = "Ingresa tu apellido."
  if (!birthDate) {
    fieldErrors.birthDate = "Ingresa tu fecha de nacimiento."
  } else if (birthDate > new Date().toISOString().slice(0, 10)) {
    fieldErrors.birthDate = "La fecha no puede ser futura."
  }
  const passwordError = validatePassword(password)
  if (passwordError) fieldErrors.password = passwordError
  if (Object.keys(fieldErrors).length) return { ok: false, values, fieldErrors }

  let requiresVerification = true
  try {
    const pending = await registerLocalUser({
      email,
      password,
      firstName,
      lastName,
      birthDate,
    })
    requiresVerification = pending.requiresEmailVerification

    // Verificación por OTP desactivada en el backend: la cuenta ya quedó activa,
    // así que iniciamos sesión de una vez y entramos al dashboard.
    if (!requiresVerification) {
      const tokens = await loginLocalUser(email, password)
      await setSession(tokens)
    }
  } catch (error) {
    return toFormState(error, values)
  }

  redirect(
    requiresVerification
      ? `/verify-email?email=${encodeURIComponent(email)}`
      : "/",
  )
}

export async function verifyOtpAction(
  _prev: FormState,
  formData: FormData,
): Promise<FormState> {
  const email = str(formData, "email")
  const otp = str(formData, "otp")
  const values = { email }

  const fieldErrors: Record<string, string> = {}
  if (!EMAIL_RE.test(email)) fieldErrors.email = "Correo inválido."
  if (!otp) fieldErrors.otp = "Ingresa el código."
  if (Object.keys(fieldErrors).length) return { ok: false, values, fieldErrors }

  try {
    const tokens = await verifyEmailOtp(email, otp)
    await setSession(tokens)
  } catch (error) {
    return toFormState(error, values)
  }

  redirect("/")
}

export async function resendOtpAction(
  _prev: FormState,
  formData: FormData,
): Promise<FormState> {
  const email = str(formData, "email")
  if (!EMAIL_RE.test(email)) {
    return { ok: false, message: "Correo inválido." }
  }

  try {
    await resendEmailOtp(email)
    return { ok: true, message: "Te enviamos un nuevo código." }
  } catch (error) {
    return toFormState(error, { email })
  }
}

export async function logoutAction(): Promise<void> {
  await endSession()
  redirect("/login")
}
