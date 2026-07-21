"use client"

import { useActionState, useEffect, useRef } from "react"
import { toast } from "sonner"

import {
  createPatientAction,
  updatePatientAction,
} from "@/lib/medical-records/actions"
import { emptyFormState } from "@/lib/forms/helpers"
import { Gender, GENDER_LABELS, type Patient } from "@/lib/medical-records/types"
import { BirthDatePicker } from "@/components/birthdate-picker"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Spinner } from "@/components/ui/spinner"
import { Textarea } from "@/components/ui/textarea"

type Props = {
  open: boolean
  onOpenChange: (open: boolean) => void
  patient?: Patient
}

export function PatientFormDialog({ open, onOpenChange, patient }: Props) {
  const isEdit = Boolean(patient)
  const [state, formAction, isPending] = useActionState(
    isEdit ? updatePatientAction : createPatientAction,
    emptyFormState,
  )
  const wasPending = useRef(false)

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

  const err = state.fieldErrors
  const v = (key: keyof Patient) =>
    (state.values?.[key] as string | undefined) ??
    (patient?.[key] != null ? String(patient[key]) : undefined)

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Editar paciente" : "Nuevo paciente"}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? "Actualiza los datos del paciente."
              : "Registra un nuevo paciente en el consultorio."}
          </DialogDescription>
        </DialogHeader>

        <form action={formAction} noValidate>
          {isEdit ? <input type="hidden" name="id" value={patient!.id} /> : null}

          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field data-invalid={!!err?.document}>
                <FieldLabel htmlFor="document">Documento</FieldLabel>
                <Input
                  id="document"
                  name="document"
                  defaultValue={v("document")}
                  readOnly={isEdit}
                  aria-invalid={!!err?.document}
                  disabled={isPending}
                />
                {err?.document && <FieldError>{err.document}</FieldError>}
              </Field>

              <Field>
                <FieldLabel htmlFor="gender">Género</FieldLabel>
                <Select
                  name="gender"
                  defaultValue={String(patient?.gender ?? Gender.Unspecified)}
                >
                  <SelectTrigger id="gender" className="w-full" disabled={isPending}>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {(
                      [Gender.Unspecified, Gender.Male, Gender.Female] as const
                    ).map((g) => (
                      <SelectItem key={g} value={String(g)}>
                        {GENDER_LABELS[g]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            </div>

            <Field data-invalid={!!err?.fullName}>
              <FieldLabel htmlFor="fullName">Nombre completo</FieldLabel>
              <Input
                id="fullName"
                name="fullName"
                defaultValue={v("fullName")}
                aria-invalid={!!err?.fullName}
                disabled={isPending}
              />
              {err?.fullName && <FieldError>{err.fullName}</FieldError>}
            </Field>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="birthDate">Fecha de nacimiento</FieldLabel>
                <BirthDatePicker
                  defaultValue={v("birthDate")}
                  disabled={isPending}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="phone">Teléfono</FieldLabel>
                <Input
                  id="phone"
                  name="phone"
                  defaultValue={v("phone")}
                  disabled={isPending}
                />
              </Field>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="email">Correo</FieldLabel>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  defaultValue={v("email")}
                  disabled={isPending}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="instagram">Instagram</FieldLabel>
                <Input
                  id="instagram"
                  name="instagram"
                  defaultValue={v("instagram")}
                  disabled={isPending}
                />
              </Field>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="occupation">Ocupación</FieldLabel>
                <Input
                  id="occupation"
                  name="occupation"
                  defaultValue={v("occupation")}
                  disabled={isPending}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="emergencyContact">
                  Contacto de emergencia
                </FieldLabel>
                <Input
                  id="emergencyContact"
                  name="emergencyContact"
                  defaultValue={v("emergencyContact")}
                  disabled={isPending}
                />
              </Field>
            </div>

            <Field>
              <FieldLabel htmlFor="address">Dirección</FieldLabel>
              <Textarea
                id="address"
                name="address"
                rows={2}
                defaultValue={v("address")}
                disabled={isPending}
              />
            </Field>
          </FieldGroup>

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
              {isEdit ? "Guardar cambios" : "Crear paciente"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
