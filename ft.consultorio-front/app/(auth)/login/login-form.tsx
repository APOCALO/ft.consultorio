"use client"

import Link from "next/link"
import { useActionState, useEffect } from "react"
import { toast } from "sonner"

import { loginAction } from "@/lib/auth/actions"
import { emptyFormState } from "@/lib/auth/form-state"
import { Button } from "@/components/ui/button"
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Spinner } from "@/components/ui/spinner"

export function LoginForm() {
  const [state, formAction, isPending] = useActionState(
    loginAction,
    emptyFormState,
  )

  useEffect(() => {
    if (state.message) toast.error(state.message)
  }, [state])

  return (
    <form action={formAction} noValidate>
      <FieldGroup>
        <Field data-invalid={!!state.fieldErrors?.email}>
          <FieldLabel htmlFor="email">Correo electrónico</FieldLabel>
          <Input
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            placeholder="tucorreo@ejemplo.com"
            defaultValue={state.values?.email}
            aria-invalid={!!state.fieldErrors?.email}
            disabled={isPending}
          />
          {state.fieldErrors?.email && (
            <FieldError>{state.fieldErrors.email}</FieldError>
          )}
        </Field>

        <Field data-invalid={!!state.fieldErrors?.password}>
          <div className="flex items-center justify-between">
            <FieldLabel htmlFor="password">Contraseña</FieldLabel>
            <Link
              href="/forgot-password"
              className="text-xs text-muted-foreground underline-offset-4 hover:underline"
            >
              ¿La olvidaste?
            </Link>
          </div>
          <Input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            aria-invalid={!!state.fieldErrors?.password}
            disabled={isPending}
          />
          {state.fieldErrors?.password && (
            <FieldError>{state.fieldErrors.password}</FieldError>
          )}
        </Field>

        <Button type="submit" className="w-full" disabled={isPending}>
          {isPending && <Spinner data-icon="inline-start" />}
          Iniciar sesión
        </Button>
      </FieldGroup>
    </form>
  )
}
