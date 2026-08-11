"use client"

import { useActionState, useEffect, useMemo, useRef, useState } from "react"
import { format } from "date-fns"
import { toast } from "sonner"
import { Plus } from "lucide-react"

import { registerPaymentAction } from "@/lib/medical-records/actions"
import { emptyFormState } from "@/lib/forms/helpers"
import {
  PAYMENT_METHOD_ITEMS,
  PAYMENT_METHOD_LABELS,
  PaymentMethod,
  type Payment,
  type Session,
} from "@/lib/medical-records/types"
import { formatDate, formatDateTime, money } from "@/lib/format"
import { DateField } from "@/components/date-field"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Field, FieldError, FieldLabel } from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Spinner } from "@/components/ui/spinner"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

export function PaymentsPanel({
  patientId,
  payments,
  sessions,
}: {
  patientId: string
  payments: Payment[]
  sessions: Session[]
}) {
  const [open, setOpen] = useState(false)
  // Igual que en las sesiones: cada apertura remonta el diálogo para que la
  // fecha por defecto sea la de hoy y el form arranque limpio.
  const [openCount, setOpenCount] = useState(0)
  const total = payments.reduce((sum, p) => sum + p.amount, 0)

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {payments.length} {payments.length === 1 ? "pago" : "pagos"} ·{" "}
          <span className="font-medium text-foreground">{money(total)}</span> recaudado
        </p>
        <Button
          size="sm"
          onClick={() => {
            setOpenCount((n) => n + 1)
            setOpen(true)
          }}
        >
          <Plus data-icon="inline-start" />
          Registrar pago
        </Button>
      </div>

      {payments.length === 0 ? (
        <Card className="p-8 text-center text-sm text-muted-foreground">
          Aún no hay pagos registrados.
        </Card>
      ) : (
        <Card className="overflow-hidden p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha</TableHead>
                <TableHead>Monto</TableHead>
                <TableHead className="hidden sm:table-cell">Método</TableHead>
                <TableHead className="hidden md:table-cell">Referencia</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {payments.map((p) => (
                <TableRow key={p.id}>
                  <TableCell>{formatDate(p.paidAt)}</TableCell>
                  <TableCell className="font-medium tabular-nums">
                    {money(p.amount)}
                  </TableCell>
                  <TableCell className="hidden text-muted-foreground sm:table-cell">
                    {PAYMENT_METHOD_LABELS[p.method]}
                  </TableCell>
                  <TableCell className="hidden text-muted-foreground md:table-cell">
                    {p.reference || "—"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      <PaymentDialog
        key={openCount}
        open={open}
        onOpenChange={setOpen}
        patientId={patientId}
        sessions={sessions}
      />
    </div>
  )
}

function PaymentDialog({
  open,
  onOpenChange,
  patientId,
  sessions,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  patientId: string
  sessions: Session[]
}) {
  const [state, formAction, isPending] = useActionState(
    registerPaymentAction,
    emptyFormState,
  )
  const wasPending = useRef(false)
  // Congelado al montar: recalcularlo en cada render cambiaría el defaultValue
  // al pasar la medianoche sobre un input ya inicializado. En hora local, que
  // `toISOString()` es UTC y en Colombia adelantaría un día por la noche.
  const [today] = useState(() => format(new Date(), "yyyy-MM-dd"))

  // `Select.Value` de base-ui pinta el valor crudo (aquí, el GUID) si no le
  // damos el mapa de etiquetas.
  const sessionItems = useMemo<Record<string, string>>(
    () => ({
      "": "Sin asociar",
      ...Object.fromEntries(
        sessions.map((s) => [
          s.id,
          `${formatDateTime(s.date)} · ${money(s.price)}`,
        ]),
      ),
    }),
    [sessions],
  )

  useEffect(() => {
    if (wasPending.current && !isPending) {
      if (state.ok) {
        toast.success(state.message ?? "Pago registrado.")
        onOpenChange(false)
      } else if (state.message) {
        toast.error(state.message)
      }
    }
    wasPending.current = isPending
  }, [state, isPending, onOpenChange])

  const err = state.fieldErrors

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Registrar pago</DialogTitle>
          <DialogDescription>Registra un pago del paciente.</DialogDescription>
        </DialogHeader>

        <form action={formAction}>
          <input type="hidden" name="patientId" value={patientId} />

          <div className="flex flex-col gap-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <Field data-invalid={!!err?.amount}>
                <FieldLabel htmlFor="amount">Monto</FieldLabel>
                <Input
                  key={state.values?.amount ?? ""}
                  id="amount"
                  name="amount"
                  type="number"
                  min={0}
                  step="1000"
                  defaultValue={state.values?.amount ?? ""}
                  aria-invalid={!!err?.amount}
                  disabled={isPending}
                />
                {err?.amount && <FieldError>{err.amount}</FieldError>}
              </Field>
              <Field>
                <FieldLabel htmlFor="method">Método</FieldLabel>
                <Select
                  name="method"
                  items={PAYMENT_METHOD_ITEMS}
                  defaultValue={String(PaymentMethod.Cash)}
                >
                  <SelectTrigger id="method" className="w-full" disabled={isPending}>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {(
                      [
                        PaymentMethod.Cash,
                        PaymentMethod.Card,
                        PaymentMethod.Transfer,
                        PaymentMethod.Other,
                      ] as const
                    ).map((m) => (
                      <SelectItem key={m} value={String(m)}>
                        {PAYMENT_METHOD_LABELS[m]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            </div>

            <Field>
              <FieldLabel htmlFor="paidAt">Fecha del pago</FieldLabel>
              <DateField
                id="paidAt"
                name="paidAt"
                defaultValue={today}
                disabled={isPending}
              />
            </Field>

            {sessions.length > 0 ? (
              <Field>
                <FieldLabel htmlFor="sessionId">Sesión (opcional)</FieldLabel>
                <Select name="sessionId" items={sessionItems} defaultValue="">
                  <SelectTrigger id="sessionId" className="w-full" disabled={isPending}>
                    <SelectValue placeholder="Sin asociar" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="">Sin asociar</SelectItem>
                    {sessions.map((s) => (
                      <SelectItem key={s.id} value={s.id}>
                        {sessionItems[s.id]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            ) : null}

            <Field>
              <FieldLabel htmlFor="reference">Referencia (opcional)</FieldLabel>
              <Input id="reference" name="reference" disabled={isPending} />
            </Field>
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
              Registrar
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
