"use client"

import { useActionState, useEffect, useRef } from "react"
import { toast } from "sonner"

import { saveMedicalRecordAction } from "@/lib/medical-records/actions"
import { emptyFormState } from "@/lib/forms/helpers"
import type { MedicalRecord } from "@/lib/medical-records/types"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
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

const TEXT_AREAS = [
  { name: "chiefComplaint", label: "Motivo de consulta" },
  { name: "currentIllness", label: "Enfermedad actual" },
  { name: "medicalDiagnosis", label: "Diagnóstico médico" },
  { name: "physiotherapyDiagnosis", label: "Diagnóstico fisioterapéutico" },
  { name: "shortGoals", label: "Objetivos a corto plazo" },
  { name: "mediumGoals", label: "Objetivos a mediano plazo" },
  { name: "longGoals", label: "Objetivos a largo plazo" },
  { name: "observations", label: "Observaciones" },
] as const

const HISTORY_TEXT = [
  { name: "surgeries", label: "Cirugías" },
  { name: "fractures", label: "Fracturas" },
  { name: "medications", label: "Medicamentos" },
  { name: "allergies", label: "Alergias" },
  { name: "otherHistory", label: "Otros antecedentes" },
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
  const val = (k: keyof MedicalRecord) =>
    (record?.[k] as string | null | undefined) ?? ""

  return (
    // `key` remonta el form cuando la historia pasa de inexistente a creada (o
    // cambia de paciente): así los `defaultValue` se aplican como valor inicial
    // y base-ui no avisa por "defaultValue cambiado tras inicializar".
    <form key={record?.id ?? "new"} action={formAction}>
      <input type="hidden" name="patientId" value={patientId} />
      {record ? <input type="hidden" name="recordId" value={record.id} /> : null}

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-sm">Valoración</CardTitle>
            {!record ? (
              <CardDescription>
                Aún no hay historia clínica. Completa y guarda para crearla.
              </CardDescription>
            ) : null}
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            {TEXT_AREAS.map((f) => (
              <Field key={f.name}>
                <FieldLabel htmlFor={f.name}>{f.label}</FieldLabel>
                <Textarea
                  id={f.name}
                  name={f.name}
                  rows={2}
                  defaultValue={val(f.name as keyof MedicalRecord)}
                  disabled={isPending}
                />
              </Field>
            ))}
          </CardContent>
        </Card>

        <Card className="h-fit">
          <CardHeader>
            <CardTitle className="text-sm">Antecedentes</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <div className="grid grid-cols-2 gap-3">
              {CONDITIONS.map((c) => (
                <label
                  key={c.name}
                  className="flex items-center gap-2 text-sm font-medium"
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

            {HISTORY_TEXT.map((f) => (
              <Field key={f.name}>
                <FieldLabel htmlFor={f.name}>{f.label}</FieldLabel>
                <Input
                  id={f.name}
                  name={f.name}
                  defaultValue={(h?.[f.name] as string | null) ?? ""}
                  disabled={isPending}
                />
              </Field>
            ))}
          </CardContent>
        </Card>
      </div>

      <div className="mt-4 flex justify-end">
        <Button type="submit" disabled={isPending}>
          {isPending && <Spinner data-icon="inline-start" />}
          {record ? "Guardar cambios" : "Crear historia clínica"}
        </Button>
      </div>
    </form>
  )
}
