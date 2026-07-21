"use client"

import { useActionState, useEffect, useRef } from "react"
import { toast } from "sonner"

import { emptyFormState, type FormState } from "@/lib/forms/helpers"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Spinner } from "@/components/ui/spinner"

type Props = {
  open: boolean
  onOpenChange: (open: boolean) => void
  action: (prev: FormState, data: FormData) => Promise<FormState>
  /** Campos ocultos enviados al action (p. ej. { id, patientId }). */
  hidden: Record<string, string>
  title: string
  description: string
  confirmLabel?: string
  destructive?: boolean
  onDone?: () => void
}

export function ConfirmActionDialog({
  open,
  onOpenChange,
  action,
  hidden,
  title,
  description,
  confirmLabel = "Confirmar",
  destructive = false,
  onDone,
}: Props) {
  const [state, formAction, isPending] = useActionState(action, emptyFormState)
  const wasPending = useRef(false)

  useEffect(() => {
    if (wasPending.current && !isPending) {
      if (state.ok) {
        toast.success(state.message ?? "Listo.")
        onOpenChange(false)
        onDone?.()
      } else if (state.message) {
        toast.error(state.message)
      }
    }
    wasPending.current = isPending
  }, [state, isPending, onOpenChange, onDone])

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <form action={formAction}>
          {Object.entries(hidden).map(([k, val]) => (
            <input key={k} type="hidden" name={k} value={val} />
          ))}
          <DialogFooter className="mt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isPending}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              variant={destructive ? "destructive" : "default"}
              disabled={isPending}
            >
              {isPending && <Spinner data-icon="inline-start" />}
              {confirmLabel}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
