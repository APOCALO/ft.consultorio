/**
 * Contratos del microservicio de historias clínicas (MsMedicalRecords).
 * El API serializa en camelCase y los enums como NÚMEROS.
 */

// --- Enums (coinciden con el backend) ----------------------------------------

export enum Gender {
  Unspecified = 0,
  Male = 1,
  Female = 2,
}

export enum PatientStatus {
  Active = 0,
  Inactive = 1,
  Discharged = 2,
}

export enum PaymentMethod {
  Cash = 0,
  Card = 1,
  Transfer = 2,
  Other = 3,
}

export const GENDER_LABELS: Record<Gender, string> = {
  [Gender.Unspecified]: "Sin especificar",
  [Gender.Male]: "Masculino",
  [Gender.Female]: "Femenino",
}

export const PATIENT_STATUS_LABELS: Record<PatientStatus, string> = {
  [PatientStatus.Active]: "Activo",
  [PatientStatus.Inactive]: "Inactivo",
  [PatientStatus.Discharged]: "Dado de alta",
}

export const PAYMENT_METHOD_LABELS: Record<PaymentMethod, string> = {
  [PaymentMethod.Cash]: "Efectivo",
  [PaymentMethod.Card]: "Tarjeta",
  [PaymentMethod.Transfer]: "Transferencia",
  [PaymentMethod.Other]: "Otro",
}

// --- Respuestas --------------------------------------------------------------

export interface Patient {
  id: string
  document: string
  fullName: string
  /** ISO `YYYY-MM-DD` (DateOnly). */
  birthDate?: string | null
  gender: Gender
  phone?: string | null
  email?: string | null
  instagram?: string | null
  occupation?: string | null
  address?: string | null
  emergencyContact?: string | null
  status: PatientStatus
  createdAt: string
  createdById: string
  updatedAt?: string | null
  updatedById?: string | null
}

export interface MedicalHistory {
  hypertension: boolean
  diabetes: boolean
  cancer: boolean
  pacemaker: boolean
  pregnancy: boolean
  surgeries?: string | null
  fractures?: string | null
  medications?: string | null
  allergies?: string | null
  otherHistory?: string | null
}

export interface MedicalRecord {
  id: string
  patientId: string
  chiefComplaint?: string | null
  currentIllness?: string | null
  medicalDiagnosis?: string | null
  physiotherapyDiagnosis?: string | null
  shortGoals?: string | null
  mediumGoals?: string | null
  longGoals?: string | null
  observations?: string | null
  history: MedicalHistory
  createdAt: string
  createdById: string
  updatedAt?: string | null
  updatedById?: string | null
}

export interface Session {
  id: string
  medicalRecordId: string
  /** ISO datetime. */
  date: string
  painScale: number
  evolution?: string | null
  treatmentPerformed?: string | null
  recommendations?: string | null
  nextAppointment?: string | null
  price: number
  paid: boolean
  createdAt: string
  createdById: string
  updatedAt?: string | null
  updatedById?: string | null
}

export interface Payment {
  id: string
  patientId: string
  sessionId?: string | null
  amount: number
  method: PaymentMethod
  reference?: string | null
  paidAt: string
  createdAt: string
  createdById: string
  updatedAt?: string | null
  updatedById?: string | null
}

export interface Dashboard {
  patients: number
  activePatients: number
  sessionsToday: number
  incomeThisMonth: number
  pendingBalance: number
}

export interface Pagination {
  totalCount: number
  pageSize: number
  pageNumber: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface PagedResult<T> {
  items: T[]
  pagination: Pagination | null
}

// --- Entradas (payloads) -----------------------------------------------------

export interface PatientInput {
  document: string
  fullName: string
  birthDate?: string | null
  gender: Gender
  phone?: string | null
  email?: string | null
  instagram?: string | null
  occupation?: string | null
  address?: string | null
  emergencyContact?: string | null
}

export interface MedicalRecordInput {
  chiefComplaint?: string | null
  currentIllness?: string | null
  medicalDiagnosis?: string | null
  physiotherapyDiagnosis?: string | null
  shortGoals?: string | null
  mediumGoals?: string | null
  longGoals?: string | null
  observations?: string | null
  history?: MedicalHistory
}

export interface SessionInput {
  date: string
  painScale: number
  evolution?: string | null
  treatmentPerformed?: string | null
  recommendations?: string | null
  nextAppointment?: string | null
  price: number
  paid: boolean
}

export interface PaymentInput {
  sessionId?: string | null
  amount: number
  method: PaymentMethod
  reference?: string | null
  paidAt?: string | null
}
