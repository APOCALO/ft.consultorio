"use client"

import { useActionState, useEffect, useRef } from "react"
import { toast } from "sonner"
import {
  ClipboardList,
  HeartPulse,
  NotebookPen,
  Save,
  Stethoscope,
  Target,
} from "lucide-react"

import { saveMedicalRecordAction } from "@/lib/medical-records/actions"
import { emptyFormState } from "@/lib/forms/helpers"
import type { MedicalRecord } from "@/lib/medical-records/types"
import { formatDateTime } from "@/lib/format"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Checkbox } from "@/components/ui/checkbox"
import { Field, FieldLabel } from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Spinner } from "@/components/ui/spinner"
import { Textarea } from "@/components/ui/textarea"

const CONDITIONS = [
  { name: "hypertension", label: "Hipertensión" },
  { name: "diabetes", label: "Diabetes" },
  { name: "cancer", label: "Cáncer" },
  { name: "pacemaker", label: "Marcapasos" },
  { name: "pregnancy", label: "Embarazo" },
] as const

export function MedicalRecordPanel({
  patientId,
  record,
}: {
  patientId: string
  record: MedicalRecord | null
}) {
  const [state, formAction, isPending] = useActionState(
    saveMedicalRecordAction,
    emptyFormState,
  )
  const wasPending = useRef(false)

  useEffect(() => {
    if (wasPending.current && !isPending) {
      if (state.ok) toast.success(state.message ?? "Guardado.")
      else if (state.message) toast.error(state.message)
    }
    wasPending.current = isPending
  }, [state, isPending])

  const h = record?.history
  const text = (k: keyof MedicalRecord) =>
    (record?.[k] as string | null | undefined) ?? ""
  const histText = (k: keyof NonNullable<typeof h>) =>
    (h?.[k] as string | null | undefined) ?? ""

  /** Textarea etiquetada (campo clínico). */
  const area = (
    name: keyof MedicalRecord,
    label: string,
    placeholder?: string,
    rows = 3,
  ) => (
    <Field>
      <FieldLabel htmlFor={name}>{label}</FieldLabel>
      <Textarea
        id={name}
        name={name}
        rows={rows}
        placeholder={placeholder}
        defaultValue={text(name)}
        disabled={isPending}
      />
    </Field>
  )

  const savedAt = record ? record.updatedAt ?? record.createdAt : null

  return (
    // `key` remonta el form cuando cambian los datos del servidor: al crear la
    // historia (null → id) y también tras cada guardado de una existente
    // (cambia `updatedAt`). Así los `defaultValue`/`defaultChecked` se aplican
    // como valor inicial y base-ui no avisa por "cambio tras inicializar".
    <form
      key={record ? `${record.id}:${record.updatedAt ?? record.createdAt}` : "new"}
      action={formAction}
      className="mx-auto max-w-4xl"
    >
      <input type="hidden" name="patientId" value={patientId} />
      {record ? <input type="hidden" name="recordId" value={record.id} /> : null}

      <div className="flex flex-col gap-4 pb-20">
        <Section
          icon={<ClipboardList />}
          title="Anamnesis"
          description="Motivo por el que consulta y evolución del problema actual."
        >
          {area(
            "chiefComplaint",
            "Motivo de consulta",
            "¿Por qué acude el paciente?",
            2,
          )}
          {area(
            "currentIllness",
            "Enfermedad actual",
            "Inicio, evolución, factores que agravan o alivian…",
          )}
        </Section>

        <Section
          icon={<Stethoscope />}
          title="Diagnóstico"
          description="Impresión médica y valoración fisioterapéutica."
        >
          <div className="grid gap-4 sm:grid-cols-2">
            {area("medicalDiagnosis", "Diagnóstico médico", undefined, 3)}
            {area(
              "physiotherapyDiagnosis",
              "Diagnóstico fisioterapéutico",
              undefined,
              3,
            )}
          </div>
        </Section>

        <Section
          icon={<Target />}
          title="Objetivos terapéuticos"
          description="Metas del plan de tratamiento por horizonte de tiempo."
        >
          <div className="grid gap-4 sm:grid-cols-3">
            {area("shortGoals", "Corto plazo", undefined, 4)}
            {area("mediumGoals", "Mediano plazo", undefined, 4)}
            {area("longGoals", "Largo plazo", undefined, 4)}
          </div>
        </Section>

        <Section
          icon={<HeartPulse />}
          title="Antecedentes"
          description="Historial clínico relevante para el tratamiento."
        >
          <div className="flex flex-col gap-5">
            <div>
              <p className="mb-2 text-xs font-medium text-muted-foreground">
                Condiciones
              </p>
              <div className="grid grid-cols-2 gap-2.5 sm:grid-cols-3 lg:grid-cols-5">
                {CONDITIONS.map((c) => (
                  <label
                    key={c.name}
                    className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-border bg-card px-3 py-2.5 text-sm font-medium transition-colors hover:bg-accent/40 has-[[data-checked]]:border-primary has-[[data-checked]]:bg-primary/5 has-disabled:cursor-not-allowed has-disabled:opacity-60"
                  >
                    <Checkbox
                      name={c.name}
                      defaultChecked={Boolean(h?.[c.name])}
                      disabled={isPending}
                    />
                    {c.label}
                  </label>
                ))}
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <HistField
                name="surgeries"
                label="Cirugías"
                value={histText("surgeries")}
                disabled={isPending}
              />
              <HistField
                name="fractures"
                label="Fracturas"
                value={histText("fractures")}
                disabled={isPending}
              />
              <HistField
                name="medications"
                label="Medicamentos"
                value={histText("medications")}
                disabled={isPending}
              />
              <HistField
                name="allergies"
                label="Alergias"
                value={histText("allergies")}
                disabled={isPending}
              />
            </div>

            <Field>
              <FieldLabel htmlFor="otherHistory">Otros antecedentes</FieldLabel>
              <Textarea
                id="otherHistory"
                name="otherHistory"
                rows={2}
                defaultValue={histText("otherHistory")}
                disabled={isPending}
              />
            </Field>
          </div>
        </Section>

        <Section
          icon={<NotebookPen />}
          title="Observaciones"
          description="Notas adicionales del profesional."
        >
          {area("observations", "Observaciones", "Cualquier detalle relevante…", 3)}
        </Section>
      </div>

      {/* Barra de acción flotante: estado + guardar, siempre accesible. */}
      <div className="sticky bottom-4 z-10 mt-4 flex items-center justify-between gap-3 rounded-xl border border-border bg-background/85 px-4 py-3 shadow-lg backdrop-blur">
        <p className="min-w-0 truncate text-xs text-muted-foreground">
          {savedAt
            ? `Última actualización: ${formatDateTime(savedAt)}`
            : "Sin guardar · completa y crea la historia clínica."}
        </p>
        <Button type="submit" disabled={isPending} className="shrink-0">
          {isPending ? (
            <Spinner data-icon="inline-start" />
          ) : (
            <Save data-icon="inline-start" />
          )}
          {record ? "Guardar cambios" : "Crear historia clínica"}
        </Button>
      </div>
    </form>
  )
}

/** Sección con encabezado (icono + título + descripción). */
function Section({
  icon,
  title,
  description,
  children,
}: {
  icon: React.ReactNode
  title: string
  description?: string
  children: React.ReactNode
}) {
  return (
    <Card>
      <CardHeader className="gap-0">
        <div className="flex items-start gap-3">
          <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary [&_svg]:size-4">
            {icon}
          </span>
          <div className="min-w-0">
            <CardTitle className="text-sm">{title}</CardTitle>
            {description ? (
              <p className="mt-0.5 text-xs text-muted-foreground">{description}</p>
            ) : null}
          </div>
        </div>
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  )
}

/** Campo de texto de antecedente (definido fuera del render para no remontar). */
function HistField({
  name,
  label,
  value,
  disabled,
}: {
  name: string
  label: string
  value: string
  disabled?: boolean
}) {
  return (
    <Field>
      <FieldLabel htmlFor={name}>{label}</FieldLabel>
      <Input id={name} name={name} defaultValue={value} disabled={disabled} />
    </Field>
  )
}
