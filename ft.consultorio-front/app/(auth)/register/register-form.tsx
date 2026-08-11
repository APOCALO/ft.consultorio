"use client"

import { useActionState, useEffect } from "react"
import { toast } from "sonner"

import { registerAction } from "@/lib/auth/actions"
import { emptyFormState } from "@/lib/auth/form-state"
import { Button } from "@/components/ui/button"
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Spinner } from "@/components/ui/spinner"
import { BirthDatePicker } from "@/components/birthdate-picker"

export function RegisterForm() {
  const [state, formAction, isPending] = useActionState(
    registerAction,
    emptyFormState,
  )

  useEffect(() => {
    if (state.message) toast.error(state.message)
  }, [state])

  const err = state.fieldErrors
  const values = state.values

  return (
    <form action={formAction} noValidate>
      <FieldGroup>
        <div className="grid grid-cols-2 gap-4">
          <Field data-invalid={!!err?.firstName}>
            <FieldLabel htmlFor="firstName">Nombre</FieldLabel>
            <Input
              key={values?.firstName ?? ""}
              id="firstName"
              name="firstName"
              autoComplete="given-name"
              defaultValue={values?.firstName ?? ""}
              aria-invalid={!!err?.firstName}
              disabled={isPending}
            />
            {err?.firstName && <FieldError>{err.firstName}</FieldError>}
          </Field>

          <Field data-invalid={!!err?.lastName}>
            <FieldLabel htmlFor="lastName">Apellido</FieldLabel>
            <Input
              key={values?.lastName ?? ""}
              id="lastName"
              name="lastName"
              autoComplete="family-name"
              defaultValue={values?.lastName ?? ""}
              aria-invalid={!!err?.lastName}
              disabled={isPending}
            />
            {err?.lastName && <FieldError>{err.lastName}</FieldError>}
          </Field>
        </div>

        <Field data-invalid={!!err?.email}>
          <FieldLabel htmlFor="email">Correo electrónico</FieldLabel>
          <Input
            key={values?.email ?? ""}
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            placeholder="tucorreo@ejemplo.com"
            defaultValue={values?.email ?? ""}
            aria-invalid={!!err?.email}
            disabled={isPending}
          />
          {err?.email && <FieldError>{err.email}</FieldError>}
        </Field>

        <Field data-invalid={!!err?.birthDate}>
          <FieldLabel htmlFor="birthDate">Fecha de nacimiento</FieldLabel>
          <BirthDatePicker
            key={values?.birthDate ?? ""}
            defaultValue={values?.birthDate}
            invalid={!!err?.birthDate}
            disabled={isPending}
          />
          {err?.birthDate && <FieldError>{err.birthDate}</FieldError>}
        </Field>

        <Field data-invalid={!!err?.password}>
          <FieldLabel htmlFor="password">Contraseña</FieldLabel>
          <Input
            id="password"
            name="password"
            type="password"
            autoComplete="new-password"
            aria-invalid={!!err?.password}
            disabled={isPending}
          />
          {err?.password ? (
            <FieldError>{err.password}</FieldError>
          ) : (
            <FieldDescription>
              Mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo.
            </FieldDescription>
          )}
        </Field>

        <Button type="submit" className="w-full" disabled={isPending}>
          {isPending && <Spinner data-icon="inline-start" />}
          Crear cuenta
        </Button>
      </FieldGroup>
    </form>
  )
}
