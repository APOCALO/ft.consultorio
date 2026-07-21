import Link from "next/link"
import { redirect } from "next/navigation"
import { ChevronLeft, ChevronRight, ShieldAlert } from "lucide-react"

import { ApiError } from "@/lib/api/types"
import { getCurrentUser } from "@/lib/auth/session"
import { getPatients } from "@/lib/medical-records/api"
import { PATIENT_STATUS_LABELS, PatientStatus } from "@/lib/medical-records/types"
import { formatDate } from "@/lib/format"
import { AppShell } from "@/components/dashboard/app-shell"
import { NewPatientButton, PatientRowActions } from "@/components/patients/patient-actions"
import { PatientsSearch } from "@/components/patients/patients-search"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

const PAGE_SIZE = 10

const STATUS_VARIANT: Record<PatientStatus, "success" | "warning" | "outline"> = {
  [PatientStatus.Active]: "success",
  [PatientStatus.Inactive]: "warning",
  [PatientStatus.Discharged]: "outline",
}

export default async function PatientsPage({
  searchParams,
}: {
  searchParams: Promise<{ search?: string; page?: string }>
}) {
  const user = await getCurrentUser()
  if (!user) redirect("/login")

  const sp = await searchParams
  const search = sp.search?.trim() || undefined
  const page = Math.max(1, Number(sp.page) || 1)

  let items: Awaited<ReturnType<typeof getPatients>>["items"] = []
  let pagination: Awaited<ReturnType<typeof getPatients>>["pagination"] = null
  let forbidden = false
  try {
    const result = await getPatients({ pageNumber: page, pageSize: PAGE_SIZE, search })
    items = result.items
    pagination = result.pagination
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) redirect("/logout")
    if (error instanceof ApiError && error.status === 403) forbidden = true
    else throw error
  }

  const buildHref = (p: number) => {
    const next = new URLSearchParams()
    if (search) next.set("search", search)
    if (p > 1) next.set("page", String(p))
    const qs = next.toString()
    return qs ? `/patients?${qs}` : "/patients"
  }

  return (
    <AppShell
      user={user}
      title="Pacientes"
      subtitle={
        pagination ? `${pagination.totalCount} en total` : "Gestión de pacientes"
      }
      actions={<NewPatientButton />}
    >
      {forbidden ? (
        <Card className="mx-auto flex max-w-lg flex-col items-center gap-2 p-8 text-center">
          <div className="flex size-11 items-center justify-center rounded-full bg-warning-soft text-warning-soft-foreground">
            <ShieldAlert className="size-5" />
          </div>
          <p className="text-sm font-medium">Acceso restringido</p>
          <p className="text-sm text-muted-foreground">
            Necesitas el rol <span className="font-medium">Admin</span> para ver
            los pacientes.
          </p>
        </Card>
      ) : (
        <>
          <PatientsSearch />

          <Card className="overflow-hidden p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Paciente</TableHead>
                  <TableHead className="hidden sm:table-cell">Documento</TableHead>
                  <TableHead className="hidden md:table-cell">Teléfono</TableHead>
                  <TableHead className="hidden lg:table-cell">Registrado</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead className="w-10" />
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.length === 0 ? (
                  <TableRow>
                    <TableCell
                      colSpan={6}
                      className="py-10 text-center text-sm text-muted-foreground"
                    >
                      {search
                        ? "No se encontraron pacientes."
                        : "Aún no hay pacientes. Crea el primero."}
                    </TableCell>
                  </TableRow>
                ) : (
                  items.map((p) => (
                    <TableRow key={p.id}>
                      <TableCell>
                        <Link
                          href={`/patients/${p.id}`}
                          className="font-medium hover:underline"
                        >
                          {p.fullName}
                        </Link>
                      </TableCell>
                      <TableCell className="hidden text-muted-foreground sm:table-cell">
                        {p.document}
                      </TableCell>
                      <TableCell className="hidden text-muted-foreground md:table-cell">
                        {p.phone || "—"}
                      </TableCell>
                      <TableCell className="hidden text-muted-foreground lg:table-cell">
                        {formatDate(p.createdAt)}
                      </TableCell>
                      <TableCell>
                        <Badge variant={STATUS_VARIANT[p.status]}>
                          {PATIENT_STATUS_LABELS[p.status]}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <PatientRowActions patient={p} />
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </Card>

          {pagination && pagination.totalPages > 1 ? (
            <div className="flex items-center justify-between">
              <p className="text-xs text-muted-foreground">
                Página {pagination.pageNumber} de {pagination.totalPages}
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!pagination.hasPreviousPage}
                  render={
                    pagination.hasPreviousPage ? (
                      <Link href={buildHref(page - 1)} />
                    ) : undefined
                  }
                >
                  <ChevronLeft data-icon="inline-start" />
                  Anterior
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!pagination.hasNextPage}
                  render={
                    pagination.hasNextPage ? (
                      <Link href={buildHref(page + 1)} />
                    ) : undefined
                  }
                >
                  Siguiente
                  <ChevronRight data-icon="inline-end" />
                </Button>
              </div>
            </div>
          ) : null}
        </>
      )}
    </AppShell>
  )
}
