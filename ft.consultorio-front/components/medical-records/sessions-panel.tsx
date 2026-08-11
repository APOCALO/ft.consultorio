"use client"

import { useActionState, useEffect, useRef, useState } from "react"
import { toast } from "sonner"
import { Pencil, Plus, Trash2 } from "lucide-react"

import {
  createSessionAction,
  deleteSessionAction,
  updateSessionAction,
} from "@/lib/medical-records/actions"
import { emptyFormState } from "@/lib/forms/helpers"
import type { Session } from "@/lib/medical-records/types"
import { formatDateTime, money } from "@/lib/format"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { ConfirmActionDialog } from "@/components/common/confirm-action-dialog"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Field, FieldLabel } from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Spinner } from "@/components/ui/spinner"
import { Switch } from "@/components/ui/switch"
import { Textarea } from "@/components/ui/textarea"

/** ISO → valor de <input type="datetime-local"> (YYYY-MM-DDTHH:mm), hora local. */
function toLocalInput(iso?: string | null): string | undefined {
  if (!iso) return undefined
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return undefined
  const off = d.getTimezoneOffset()
  return new Date(d.getTime() - off * 60000).toISOString().slice(0, 16)
}

export function SessionsPanel({
  patientId,
  recordId,
  sessions,
}: {
  patientId: string
  recordId: string | null
  sessions: Session[]
}) {
  const [creating, setCreating] = useState(false)
  // Cuenta las aperturas: al usarla como `key` del diálogo, cada «Nueva sesión»
  // arranca con una instancia limpia (fecha actual y estado del form vacío).
  // Solo cambia al abrir, así que la animación de cierre se conserva.
  const [openCount, setOpenCount] = useState(0)

  if (!recordId) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-sm">Sesiones</CardTitle>
          <CardDescription>
            Primero crea la historia clínica en la pestaña «Historia clínica»
            para poder registrar sesiones.
          </CardDescription>
        </CardHeader>
      </Card>
    )
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {sessions.length} {sessions.length === 1 ? "sesión" : "sesiones"}
        </p>
        <Button
          size="sm"
          onClick={() => {
            setOpenCount((n) => n + 1)
            setCreating(true)
          }}
        >
          <Plus data-icon="inline-start" />
          Nueva sesión
        </Button>
      </div>

      {sessions.length === 0 ? (
        <Card className="p-8 text-center text-sm text-muted-foreground">
          Aún no hay sesiones registradas.
        </Card>
      ) : (
        <div className="flex flex-col gap-3">
          {sessions.map((s) => (
            <SessionCard key={s.id} session={s} patientId={patientId} />
          ))}
        </div>
      )}

      <SessionFormDialog
        key={openCount}
        open={creating}
        onOpenChange={setCreating}
        patientId={patientId}
        recordId={recordId}
      />
    </div>
  )
}

function SessionCard({
  session,
  patientId,
}: {
  session: Session
  patientId: string
}) {
  const [edit, setEdit] = useState(false)
  const [remove, setRemove] = useState(false)

  return (
    <Card className="p-4">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium">{formatDateTime(session.date)}</p>
          <p className="text-xs text-muted-foreground">
            Dolor {session.painScale}/10 · {money(session.price)}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant={session.paid ? "success" : "warning"}>
            {session.paid ? "Pagada" : "Pendiente"}
          </Badge>
          <Button
            variant="ghost"
            size="icon-sm"
            aria-label="Editar"
            onClick={() => setEdit(true)}
          >
            <Pencil />
          </Button>
          <Button
            variant="ghost"
            size="icon-sm"
            aria-label="Eliminar"
            onClick={() => setRemove(true)}
          >
            <Trash2 />
          </Button>
        </div>
      </div>

      {(session.evolution ||
        session.treatmentPerformed ||
        session.recommendations) && (
        <div className="mt-3 grid gap-2 text-sm sm:grid-cols-3">
          <Detail label="Evolución" value={session.evolution} />
          <Detail label="Tratamiento" value={session.treatmentPerformed} />
          <Detail label="Recomendaciones" value={session.recommendations} />
        </div>
      )}

      <SessionFormDialog
        open={edit}
        onOpenChange={setEdit}
        patientId={patientId}
        recordId={session.medicalRecordId}
        session={session}
      />
      <ConfirmActionDialog
        open={remove}
        onOpenChange={setRemove}
        action={deleteSessionAction}
        hidden={{ id: session.id, patientId }}
        title="Eliminar sesión"
        description="Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        destructive
      />
    </Card>
  )
}

function Detail({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null
  return (
    <div>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="whitespace-pre-wrap">{value}</p>
    </div>
  )
}

function SessionFormDialog({
  open,
  onOpenChange,
  patientId,
  recordId,
  session,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  patientId: string
  recordId: string
  session?: Session
}) {
  const isEdit = Boolean(session)
  const [state, formAction, isPending] = useActionState(
    isEdit ? updateSessionAction : createSessionAction,
    emptyFormState,
  )
  const wasPending = useRef(false)
  // El «ahora» se congela al montar. Recalcularlo en cada render haría cambiar
  // el defaultValue al pasar el minuto sobre un input ya inicializado, que es
  // justo de lo que avisa base-ui. Al crear, el diálogo se remonta en cada
  // apertura (ver `openCount`), así que la hora sigue siendo la actual.
  const [now] = useState(() => toLocalInput(new Date().toISOString()))
  const defaultDate = toLocalInput(session?.date) ?? now

  useEffect(() => {
    if (wasPending.current && !isPending) {
      if (state.ok) {
        toast.success(state.message ?? "Guardado.")
        onOpenChange(false)
      } else if (state.message) {
        toast.error(state.message)
      }
    }
    wasPending.current = isPending
  }, [state, isPending, onOpenChange])

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Editar sesión" : "Nueva sesión"}</DialogTitle>
          <DialogDescription>
            Registra la evolución y el cobro de la sesión.
          </DialogDescription>
        </DialogHeader>

        {/* `key` remonta el form cuando cambian los datos del servidor (tras
            guardar cambia `updatedAt`). El diálogo vive montado dentro de la
            card, así que sin esto los `defaultValue`/`defaultChecked` mutarían
            sobre inputs ya inicializados y base-ui avisaría por
            "cambio tras inicializar". */}
        <form
          key={session ? `${session.id}:${session.updatedAt ?? session.createdAt}` : "new"}
          action={formAction}
        >
          <input type="hidden" name="patientId" value={patientId} />
          {isEdit ? (
            <input type="hidden" name="id" value={session!.id} />
          ) : (
            <input type="hidden" name="recordId" value={recordId} />
          )}

          <div className="flex flex-col gap-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="date">Fecha y hora</FieldLabel>
                <Input
                  id="date"
                  name="date"
                  type="datetime-local"
                  defaultValue={defaultDate}
                  disabled={isPending}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="painScale">Escala de dolor (0-10)</FieldLabel>
                <Input
                  id="painScale"
                  name="painScale"
                  type="number"
                  min={0}
                  max={10}
                  defaultValue={session?.painScale ?? 0}
                  disabled={isPending}
                />
              </Field>
            </div>

            <Field>
              <FieldLabel htmlFor="evolution">Evolución</FieldLabel>
              <Textarea
                id="evolution"
                name="evolution"
                rows={2}
                defaultValue={session?.evolution ?? undefined}
                disabled={isPending}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="treatmentPerformed">
                Tratamiento realizado
              </FieldLabel>
              <Textarea
                id="treatmentPerformed"
                name="treatmentPerformed"
                rows={2}
                defaultValue={session?.treatmentPerformed ?? undefined}
                disabled={isPending}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="recommendations">Recomendaciones</FieldLabel>
              <Textarea
                id="recommendations"
                name="recommendations"
                rows={2}
                defaultValue={session?.recommendations ?? undefined}
                disabled={isPending}
              />
            </Field>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="nextAppointment">Próxima cita</FieldLabel>
                <Input
                  id="nextAppointment"
                  name="nextAppointment"
                  type="datetime-local"
                  defaultValue={toLocalInput(session?.nextAppointment)}
                  disabled={isPending}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="price">Valor</FieldLabel>
                <Input
                  id="price"
                  name="price"
                  type="number"
                  min={0}
                  step="1000"
                  defaultValue={session?.price ?? 0}
                  disabled={isPending}
                />
              </Field>
            </div>

            <label className="flex items-center gap-2 text-sm font-medium">
              <Switch
                name="paid"
                defaultChecked={session?.paid ?? false}
                disabled={isPending}
              />
              Sesión pagada
            </label>
          </div>

          <DialogFooter className="mt-6">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isPending}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending && <Spinner data-icon="inline-start" />}
              {isEdit ? "Guardar" : "Registrar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
