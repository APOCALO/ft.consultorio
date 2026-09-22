import Link from "next/link"
import { notFound, redirect } from "next/navigation"
import { ArrowLeft } from "lucide-react"

import { ApiError } from "@/lib/api/types"
import { getCurrentUser } from "@/lib/auth/session"
import {
  getMedicalRecord,
  getPatient,
  getSessions,
} from "@/lib/medical-records/api"
import {
  GENDER_LABELS,
  PATIENT_STATUS_LABELS,
  PatientStatus,
  type MedicalRecord,
  type Session,
} from "@/lib/medical-records/types"
import { formatDate } from "@/lib/format"
import { AppShell } from "@/components/dashboard/app-shell"
import { PatientRowActions } from "@/components/patients/patient-actions"
import { MedicalRecordPanel } from "@/components/medical-records/medical-record-panel"
import { SessionsPanel } from "@/components/medical-records/sessions-panel"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

const STATUS_VARIANT: Record<PatientStatus, "success" | "warning" | "outline"> = {
  [PatientStatus.Active]: "success",
  [PatientStatus.Inactive]: "warning",
  [PatientStatus.Discharged]: "outline",
}

export default async function PatientDetailPage({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>
  searchParams: Promise<{ tab?: string }>
}) {
  const user = await getCurrentUser()
  if (!user) redirect("/login")

  const { id } = await params
  const { tab } = await searchParams
  const initialTab = tab === "sessions" || tab === "data" ? tab : "record"

  const patient = await getPatient(id).catch((error) => {
    if (error instanceof ApiError && error.status === 401) redirect("/logout")
    if (error instanceof ApiError && (error.status === 404 || error.status === 403)) {
      notFound()
    }
    throw error
  })

  // La historia clínica puede no existir todavía (404).
  let record: MedicalRecord | null = null
  try {
    record = await getMedicalRecord(id)
  } catch (error) {
    if (!(error instanceof ApiError && error.status === 404)) throw error
  }

  const sessions = record
    ? await getSessions(record.id)
    : ([] as Session[])

  return (
    <AppShell
      user={user}
      title={patient.fullName}
      subtitle={`Doc. ${patient.document} · ${GENDER_LABELS[patient.gender]}`}
      actions={
        <>
          <Button
            variant="ghost"
            size="sm"
            className="text-muted-foreground"
            render={<Link href="/patients" />}
          >
            <ArrowLeft data-icon="inline-start" />
            <span className="hidden sm:inline">Pacientes</span>
          </Button>
          <Badge variant={STATUS_VARIANT[patient.status]}>
            {PATIENT_STATUS_LABELS[patient.status]}
          </Badge>
          <PatientRowActions patient={patient} />
        </>
      }
    >
      <Tabs defaultValue={initialTab}>
        <TabsList>
          <TabsTrigger value="record">Historia clínica</TabsTrigger>
          <TabsTrigger value="sessions">Sesiones</TabsTrigger>
          <TabsTrigger value="data">Datos</TabsTrigger>
        </TabsList>

        <TabsContent value="record" className="mt-4">
          <MedicalRecordPanel patientId={id} record={record} />
        </TabsContent>

        <TabsContent value="sessions" className="mt-4">
          <SessionsPanel
            patientId={id}
            recordId={record?.id ?? null}
            sessions={sessions}
          />
        </TabsContent>

        <TabsContent value="data" className="mt-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-sm">Datos del paciente</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-x-6 gap-y-4 sm:grid-cols-2 lg:grid-cols-3">
              <Info label="Documento" value={patient.document} />
              <Info label="Nombre completo" value={patient.fullName} />
              <Info label="Género" value={GENDER_LABELS[patient.gender]} />
              <Info label="Fecha de nacimiento" value={formatDate(patient.birthDate)} />
              <Info label="Teléfono" value={patient.phone} />
              <Info label="Correo" value={patient.email} />
              <Info label="Instagram" value={patient.instagram} />
              <Info label="Ocupación" value={patient.occupation} />
              <Info label="Contacto de emergencia" value={patient.emergencyContact} />
              <Info label="Dirección" value={patient.address} />
              <Info label="Registrado" value={formatDate(patient.createdAt)} />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </AppShell>
  )
}

function Info({ label, value }: { label: string; value?: string | null }) {
  return (
    <div>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-sm">{value || "—"}</p>
    </div>
  )
}
