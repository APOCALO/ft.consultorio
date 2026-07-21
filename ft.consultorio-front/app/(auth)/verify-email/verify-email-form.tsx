"use client"

import { useActionState, useEffect, useRef, useState } from "react"
import { toast } from "sonner"

import { resendOtpAction, verifyOtpAction } from "@/lib/auth/actions"
import { emptyFormState } from "@/lib/auth/form-state"
import { Button } from "@/components/ui/button"
import { Field, FieldError } from "@/components/ui/field"
import {
  InputOTP,
  InputOTPGroup,
  InputOTPSlot,
} from "@/components/ui/input-otp"
import { Spinner } from "@/components/ui/spinner"

const OTP_LENGTH = 6

export function VerifyEmailForm({ email }: { email: string }) {
  const [otp, setOtp] = useState("")
  const formRef = useRef<HTMLFormElement>(null)

  const [state, formAction, isPending] = useActionState(
    verifyOtpAction,
    emptyFormState,
  )
  const [resendState, resendAction, isResending] = useActionState(
    resendOtpAction,
    emptyFormState,
  )

  useEffect(() => {
    if (state.message) toast.error(state.message)
  }, [state])

  useEffect(() => {
    if (!resendState.message) return
    if (resendState.ok) {
      toast.success(resendState.message)
    } else {
      toast.error(resendState.message)
    }
  }, [resendState])

  return (
    <div className="flex flex-col gap-4">
      <form ref={formRef} action={formAction} noValidate>
        <input type="hidden" name="email" value={email} />
        <input type="hidden" name="otp" value={otp} />

        <Field data-invalid={!!state.fieldErrors?.otp}>
          <InputOTP
            maxLength={OTP_LENGTH}
            value={otp}
            onChange={setOtp}
            aria-invalid={!!state.fieldErrors?.otp}
            disabled={isPending}
            onComplete={() => formRef.current?.requestSubmit()}
            containerClassName="w-full justify-center"
          >
            <InputOTPGroup>
              {Array.from({ length: OTP_LENGTH }).map((_, i) => (
                <InputOTPSlot key={i} index={i} />
              ))}
            </InputOTPGroup>
          </InputOTP>
          {state.fieldErrors?.otp && (
            <FieldError className="text-center">
              {state.fieldErrors.otp}
            </FieldError>
          )}
        </Field>

        <Button
          type="submit"
          className="mt-4 w-full"
          disabled={isPending || otp.length < OTP_LENGTH}
        >
          {isPending && <Spinner data-icon="inline-start" />}
          Verificar y entrar
        </Button>
      </form>

      <form action={resendAction} className="text-center">
        <input type="hidden" name="email" value={email} />
        <Button
          type="submit"
          variant="link"
          size="sm"
          disabled={isResending}
          className="text-muted-foreground"
        >
          {isResending && <Spinner data-icon="inline-start" />}
          Reenviar código
        </Button>
      </form>
    </div>
  )
}
