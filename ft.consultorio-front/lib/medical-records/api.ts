import "server-only"

import { serverFetch, serverFetchPaged } from "@/lib/api/server"
import { ApiError } from "@/lib/api/types"
import { getValidAccessToken } from "@/lib/auth/session"
import type {
  Dashboard,
  MedicalRecord,
  MedicalRecordInput,
  PagedResult,
  Patient,
  PatientInput,
  Session,
  SessionInput,
} from "./types"

/**
 * Cliente del microservicio de historias clínicas (MsMedicalRecords) vía Gateway.
 * Todos los endpoints exigen `[Authorize(Roles = "Admin")]`, así que cada llamada
 * adjunta un access token válido (renovado si hace falta) desde la sesión BFF.
 * Se usa exclusivamente desde Server Actions / componentes servidor.
 */

async function token(): Promise<string> {
  const access = await getValidAccessToken()
  if (!access) {
    throw new ApiError(401, {
      title: "Sesión expirada",
      detail: "Inicia sesión de nuevo para continuar.",
      status: 401,
    })
  }
  return access
}

interface Opts {
  method?: "GET" | "POST" | "PUT" | "DELETE"
  body?: unknown
}

async function mr<T>(path: string, opts: Opts = {}): Promise<T> {
  return serverFetch<T>(path, {
    ...opts,
    service: "medical-records",
    accessToken: await token(),
  })
}

// --- Dashboard ---------------------------------------------------------------

export function getDashboard(): Promise<Dashboard> {
  return mr<Dashboard>("/dashboard")
}

// --- Pacientes ---------------------------------------------------------------

export async function getPatients(params: {
  pageNumber?: number
  pageSize?: number
  search?: string
}): Promise<PagedResult<Patient>> {
  const qs = new URLSearchParams()
  qs.set("pageNumber", String(params.pageNumber ?? 1))
  qs.set("pageSize", String(params.pageSize ?? 10))
  if (params.search) qs.set("search", params.search)

  const { data, pagination } = await serverFetchPaged<Patient[]>(
    `/patients?${qs.toString()}`,
    { service: "medical-records", accessToken: await token() },
  )
  return { items: data ?? [], pagination: pagination ?? null }
}

export function getPatient(id: string): Promise<Patient> {
  return mr<Patient>(`/patients/${id}`)
}

export function createPatient(input: PatientInput): Promise<Patient> {
  return mr<Patient>("/patients", { method: "POST", body: input })
}

export function updatePatient(id: string, input: PatientInput): Promise<Patient> {
  return mr<Patient>(`/patients/${id}`, {
    method: "PUT",
    body: { id, ...input },
  })
}

export function deletePatient(id: string): Promise<void> {
  return mr<void>(`/patients/${id}`, { method: "DELETE" })
}

export function dischargePatient(id: string): Promise<Patient> {
  return mr<Patient>(`/patients/${id}/discharge`, { method: "POST" })
}

// --- Historia clínica --------------------------------------------------------

export function getMedicalRecord(patientId: string): Promise<MedicalRecord> {
  return mr<MedicalRecord>(`/patients/${patientId}/medical-record`)
}

export function createMedicalRecord(
  patientId: string,
  input: MedicalRecordInput,
): Promise<MedicalRecord> {
  return mr<MedicalRecord>(`/patients/${patientId}/medical-record`, {
    method: "POST",
    body: input,
  })
}

export function updateMedicalRecord(
  id: string,
  input: MedicalRecordInput,
): Promise<MedicalRecord> {
  return mr<MedicalRecord>(`/medical-records/${id}`, {
    method: "PUT",
    body: { id, ...input },
  })
}

// --- Sesiones ----------------------------------------------------------------

export function getSessions(recordId: string): Promise<Session[]> {
  return mr<Session[]>(`/medical-records/${recordId}/sessions`)
}

export function createSession(
  recordId: string,
  input: SessionInput,
): Promise<Session> {
  return mr<Session>(`/medical-records/${recordId}/sessions`, {
    method: "POST",
    body: input,
  })
}

export function updateSession(id: string, input: SessionInput): Promise<Session> {
  return mr<Session>(`/sessions/${id}`, {
    method: "PUT",
    body: { id, ...input },
  })
}

export function deleteSession(id: string): Promise<void> {
  return mr<void>(`/sessions/${id}`, { method: "DELETE" })
}
