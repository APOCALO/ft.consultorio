"use client"

import { useLayoutEffect, useRef, useState } from "react"

import { Input } from "@/components/ui/input"
import { copDigits, formatCopInput } from "@/lib/format"
import { caretForMasked } from "@/lib/input-mask"
import { cn } from "@/lib/utils"

const MAX_DIGITS = 12

type MoneyFieldProps = {
  id?: string
  name: string
  /** Número, dígitos o texto ya formateado (`50.000`). */
  defaultValue?: string | number | null
  invalid?: boolean
  disabled?: boolean
  placeholder?: string
}

/**
 * Monto en pesos colombianos. Muestra `50.000` mientras se escribe y envía
 * los dígitos (`50000`) en un input oculto para que el servidor no lea el
 * punto de miles como decimal.
 */
export function MoneyField({
  id,
  name,
  defaultValue,
  invalid,
  disabled,
  placeholder = "",
}: MoneyFieldProps) {
  const [digits, setDigits] = useState(() => {
    const next = copDigits(defaultValue).slice(0, MAX_DIGITS)
    // Un 0 inicial obliga a borrarlo antes de pegar el monto.
    return next === "0" ? "" : next
  })
  const inputRef = useRef<HTMLInputElement>(null)
  const caret = useRef<number | null>(null)
  const formatted = formatCopInput(digits)

  useLayoutEffect(() => {
    const el = inputRef.current
    const pos = caret.current
    if (!el || pos == null) return
    el.setSelectionRange(pos, pos)
    caret.current = null
  }, [formatted])

  return (
    <div className="relative">
      <input type="hidden" name={name} value={digits} />
      <span
        aria-hidden="true"
        className={cn(
          "pointer-events-none absolute inset-y-0 left-3 flex items-center text-sm text-muted-foreground",
          disabled && "opacity-50",
        )}
      >
        $
      </span>
      <Input
        ref={inputRef}
        id={id ?? name}
        value={formatted}
        onChange={(e) => {
          const raw = e.target.value
          const start = e.target.selectionStart ?? raw.length
          const next = copDigits(raw).slice(0, MAX_DIGITS)
          const masked = formatCopInput(next)
          const pos = caretForMasked(raw, start, masked)
          caret.current = pos
          if (next === digits) {
            e.target.value = masked
            e.target.setSelectionRange(pos, pos)
            return
          }
          setDigits(next)
        }}
        onKeyDown={(e) => {
          // El punto es separador de miles, no un decimal que la persona escribe.
          if (e.key === "." || e.key === ",") e.preventDefault()
        }}
        placeholder={placeholder}
        inputMode="numeric"
        autoComplete="off"
        spellCheck={false}
        disabled={disabled}
        aria-invalid={invalid}
        className="pl-7 tabular-nums"
      />
    </div>
  )
}
