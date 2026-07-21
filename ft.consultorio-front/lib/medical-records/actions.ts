"use server"

import { revalidatePath } from "next/cache"

import {
  bool,
  str,
  strOrNull,
  toFormState,
  type FormState,
} from "@/lib/forms/helpers"
import * as api from "./api"
import {
  Gender,
  PaymentMethod,
  type MedicalHistory,
  type PatientInput,
  type SessionInput,
} from "./types"

// --- Helpers -----------------------------------------------------------------

function num(data: FormData, key: string, fallback = 0): number {
  const raw = str(data, key)
  if (!raw) return fallback
  const n = Number(raw)
  return Number.isFinite(n) ? n : fallback
}

function parsePatient(data: FormData): PatientInput {
  return {
    document: str(data, "document"),
    fullName: str(data, "fullName"),
    birthDate: strOrNull(data, "birthDate"),
    gender: num(data, "gender", Gender.Unspecified) as Gender,
    phone: strOrNull(data, "phone"),
    email: strOrNull(data, "email"),
    instagram: strOrNull(data, "instagram"),
    occupation: strOrNull(data, "occupation"),
    address: strOrNull(data, "address"),
    emergencyContact: strOrNull(data, "emergencyContact"),
  }
}

function patientValues(data: FormData): Record<string, string> {
  return {
    document: str(data, "document"),
    fullName: str(data, "fullName"),
    birthDate: str(data, "birthDate"),
    gender: str(data, "gender"),
    phone: str(data, "phone"),
    email: str(data, "email"),
    instagram: str(data, "instagram"),
    occupation: str(data, "occupation"),
    address: str(data, "address"),
    emergencyContact: str(data, "emergencyContact"),
  }
}

// --- Pacientes ---------------------------------------------------------------

export async function createPatientAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const values = patientValues(data)
  const fieldErrors: Record<string, string> = {}
  if (!values.document) fieldErrors.document = "Ingresa el documento."
  if (!values.fullName) fieldErrors.fullName = "Ingresa el nombre completo."
  if (Object.keys(fieldErrors).length) return { ok: false, values, fieldErrors }

  try {
    await api.createPatient(parsePatient(data))
  } catch (error) {
    return toFormState(error, values)
  }
  revalidatePath("/patients")
  revalidatePath("/")
  return { ok: true, message: "Paciente creado." }
}

export async function updatePatientAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const id = str(data, "id")
  const values = patientValues(data)
  const fieldErrors: Record<string, string> = {}
  if (!id) return { ok: false, values, message: "Paciente inválido." }
  if (!values.fullName) fieldErrors.fullName = "Ingresa el nombre completo."
  if (Object.keys(fieldErrors).length) return { ok: false, values, fieldErrors }

  try {
    // El backend no permite cambiar el documento en update.
    await api.updatePatient(id, { ...parsePatient(data), document: values.document })
  } catch (error) {
    return toFormState(error, values)
  }
  revalidatePath("/patients")
  revalidatePath(`/patients/${id}`)
  return { ok: true, message: "Paciente actualizado." }
}

export async function deletePatientAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const id = str(data, "id")
  if (!id) return { ok: false, message: "Paciente inválido." }
  try {
    await api.deletePatient(id)
  } catch (error) {
    return toFormState(error)
  }
  revalidatePath("/patients")
  revalidatePath("/")
  return { ok: true, message: "Paciente eliminado." }
}

export async function dischargePatientAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const id = str(data, "id")
  if (!id) return { ok: false, message: "Paciente inválido." }
  try {
    await api.dischargePatient(id)
  } catch (error) {
    return toFormState(error)
  }
  revalidatePath("/patients")
  revalidatePath(`/patients/${id}`)
  return { ok: true, message: "Paciente dado de alta." }
}

// --- Historia clínica --------------------------------------------------------

function parseHistory(data: FormData): MedicalHistory {
  return {
    hypertension: bool(data, "hypertension"),
    diabetes: bool(data, "diabetes"),
    cancer: bool(data, "cancer"),
    pacemaker: bool(data, "pacemaker"),
    pregnancy: bool(data, "pregnancy"),
    surgeries: strOrNull(data, "surgeries"),
    fractures: strOrNull(data, "fractures"),
    medications: strOrNull(data, "medications"),
    allergies: strOrNull(data, "allergies"),
    otherHistory: strOrNull(data, "otherHistory"),
  }
}

/** Crea o actualiza la historia clínica según venga `recordId`. */
export async function saveMedicalRecordAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const patientId = str(data, "patientId")
  const recordId = str(data, "recordId")
  if (!patientId) return { ok: false, message: "Paciente inválido." }

  const input = {
    chiefComplaint: strOrNull(data, "chiefComplaint"),
    currentIllness: strOrNull(data, "currentIllness"),
    medicalDiagnosis: strOrNull(data, "medicalDiagnosis"),
    physiotherapyDiagnosis: strOrNull(data, "physiotherapyDiagnosis"),
    shortGoals: strOrNull(data, "shortGoals"),
    mediumGoals: strOrNull(data, "mediumGoals"),
    longGoals: strOrNull(data, "longGoals"),
    observations: strOrNull(data, "observations"),
    history: parseHistory(data),
  }

  try {
    if (recordId) {
      await api.updateMedicalRecord(recordId, input)
    } else {
      await api.createMedicalRecord(patientId, input)
    }
  } catch (error) {
    return toFormState(error)
  }
  revalidatePath(`/patients/${patientId}`)
  return { ok: true, message: "Historia clínica guardada." }
}

// --- Sesiones ----------------------------------------------------------------

function parseSession(data: FormData): SessionInput {
  return {
    date: str(data, "date") || new Date().toISOString(),
    painScale: num(data, "painScale", 0),
    evolution: strOrNull(data, "evolution"),
    treatmentPerformed: strOrNull(data, "treatmentPerformed"),
    recommendations: strOrNull(data, "recommendations"),
    nextAppointment: strOrNull(data, "nextAppointment"),
    price: num(data, "price", 0),
    paid: bool(data, "paid"),
  }
}

export async function createSessionAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const recordId = str(data, "recordId")
  const patientId = str(data, "patientId")
  if (!recordId) return { ok: false, message: "Historia clínica inválida." }
  try {
    await api.createSession(recordId, parseSession(data))
  } catch (error) {
    return toFormState(error)
  }
  if (patientId) revalidatePath(`/patients/${patientId}`)
  return { ok: true, message: "Sesión registrada." }
}

export async function updateSessionAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const id = str(data, "id")
  const patientId = str(data, "patientId")
  if (!id) return { ok: false, message: "Sesión inválida." }
  try {
    await api.updateSession(id, parseSession(data))
  } catch (error) {
    return toFormState(error)
  }
  if (patientId) revalidatePath(`/patients/${patientId}`)
  return { ok: true, message: "Sesión actualizada." }
}

export async function deleteSessionAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const id = str(data, "id")
  const patientId = str(data, "patientId")
  if (!id) return { ok: false, message: "Sesión inválida." }
  try {
    await api.deleteSession(id)
  } catch (error) {
    return toFormState(error)
  }
  if (patientId) revalidatePath(`/patients/${patientId}`)
  return { ok: true, message: "Sesión eliminada." }
}

// --- Pagos -------------------------------------------------------------------

export async function registerPaymentAction(
  _prev: FormState,
  data: FormData,
): Promise<FormState> {
  const patientId = str(data, "patientId")
  if (!patientId) return { ok: false, message: "Paciente inválido." }

  const values = { amount: str(data, "amount"), method: str(data, "method") }
  if (!values.amount || Number(values.amount) <= 0) {
    return { ok: false, values, fieldErrors: { amount: "Ingresa un monto válido." } }
  }

  try {
    await api.registerPayment(patientId, {
      sessionId: strOrNull(data, "sessionId"),
      amount: num(data, "amount", 0),
      method: num(data, "method", PaymentMethod.Cash) as PaymentMethod,
      reference: strOrNull(data, "reference"),
      paidAt: strOrNull(data, "paidAt"),
    })
  } catch (error) {
    return toFormState(error, values)
  }
  revalidatePath(`/patients/${patientId}`)
  return { ok: true, message: "Pago registrado." }
}
