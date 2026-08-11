"use client"

import { useState } from "react"
import { format, isValid, parse } from "date-fns"
import { es } from "date-fns/locale"
import { CalendarIcon } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import { Input } from "@/components/ui/input"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"

/** Valor que el backend espera (DateOnly). */
const ISO = "yyyy-MM-dd"
/** Formato visible en español. */
const DISPLAY = "dd/MM/yyyy"
/** Hora visible y enviada (24 h, sin segundos). */
const TIME = "HH:mm"

/** Fecha base para `parse`: solo rellena los campos ausentes del patrón. */
const REF = new Date(2000, 0, 1)

/**
 * Formatos que aceptamos al escribir o pegar. Se valida con ida y vuelta
 * (`format(parse(s)) === s`) para que un patrón no «rescate» texto de otro.
 */
const DATE_PATTERNS = [
  DISPLAY,
  "d/M/yyyy",
  "dd-MM-yyyy",
  "d-M-yyyy",
  "dd.MM.yyyy",
  "d.M.yyyy",
  ISO,
  "ddMMyyyy",
]

const TIME_PATTERNS = [TIME, "H:mm", "HHmm"]

function parseWith(text: string, patterns: string[]): Date | undefined {
  const s = text.trim()
  if (!s) return undefined
  for (const pattern of patterns) {
    const d = parse(s, pattern, REF)
    if (isValid(d) && format(d, pattern) === s) return d
  }
  return undefined
}

/** Texto → `Date` aceptando dd/MM/yyyy, yyyy-MM-dd, ddMMyyyy, etc. */
function parseDateText(text: string): Date | undefined {
  return parseWith(text, DATE_PATTERNS)
}

/** Texto → `HH:mm` normalizado; tolera `9:05`, `0905` y `09:05:00`. */
function parseTimeText(text: string): string | undefined {
  const s = text.trim().replace(/^(\d{1,2}:\d{2}):\d{2}$/, "$1")
  const d = parseWith(s, TIME_PATTERNS)
  return d ? format(d, TIME) : undefined
}

/** ISO del backend → `{ date: "dd/MM/yyyy", time: "HH:mm" }` en hora local. */
function splitIso(iso?: string | null): { date: string; time: string } {
  if (!iso) return { date: "", time: "" }
  // `yyyy-MM-dd` sin hora se interpretaría como UTC y podría retroceder un día.
  const d = /^\d{4}-\d{2}-\d{2}$/.test(iso)
    ? parse(iso, ISO, REF)
    : new Date(iso)
  if (!isValid(d)) return { date: "", time: "" }
  return { date: format(d, DISPLAY), time: format(d, TIME) }
}

type CalendarLimits = {
  /** Primera fecha seleccionable. */
  minDate?: Date
  /** Última fecha seleccionable. */
  maxDate?: Date
  /** Mes que abre el calendario cuando aún no hay fecha. */
  defaultMonth?: Date
}

function DatePopover({
  value,
  onSelect,
  disabled,
  minDate,
  maxDate,
  defaultMonth,
}: CalendarLimits & {
  value?: Date
  onSelect: (date: Date) => void
  disabled?: boolean
}) {
  const [open, setOpen] = useState(false)

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            disabled={disabled}
            aria-label="Abrir calendario"
            tabIndex={-1}
            className="absolute inset-y-0 right-1 my-auto text-muted-foreground"
          />
        }
      >
        <CalendarIcon />
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="end">
        <Calendar
          mode="single"
          selected={value}
          onSelect={(picked) => {
            if (!picked) return
            onSelect(picked)
            setOpen(false)
          }}
          defaultMonth={value ?? defaultMonth}
          captionLayout="dropdown"
          startMonth={minDate ?? new Date(1920, 0)}
          endMonth={maxDate ?? new Date(new Date().getFullYear() + 5, 11)}
          disabled={[
            ...(minDate ? [{ before: minDate }] : []),
            ...(maxDate ? [{ after: maxDate }] : []),
          ]}
          locale={es}
          autoFocus
        />
      </PopoverContent>
    </Popover>
  )
}

type DateFieldProps = CalendarLimits & {
  id?: string
  name: string
  /** `yyyy-MM-dd` (o ISO completo); se muestra como dd/MM/yyyy. */
  defaultValue?: string | null
  invalid?: boolean
  disabled?: boolean
}

/**
 * Campo de fecha en formato local (dd/MM/yyyy). Un `<input type="date">` nativo
 * usa el formato del locale del navegador (en-US → MM/dd/yyyy), no es
 * configurable y en varios navegadores no deja copiar ni pegar. Aquí el usuario
 * escribe o pega libremente y el calendario es solo un atajo; el valor viaja
 * como `yyyy-MM-dd` en un input oculto (sin desfase de zona horaria).
 */
export function DateField({
  id,
  name,
  defaultValue,
  invalid,
  disabled,
  minDate,
  maxDate,
  defaultMonth,
}: DateFieldProps) {
  const [text, setText] = useState(() => splitIso(defaultValue).date)
  const date = parseDateText(text)

  return (
    <div className="relative">
      <input type="hidden" name={name} value={date ? format(date, ISO) : ""} />
      <Input
        id={id ?? name}
        value={text}
        onChange={(e) => setText(e.target.value)}
        onBlur={() => {
          // Normaliza lo escrito (`3/5/2024`, `2024-05-03`…) a dd/MM/yyyy.
          if (date) setText(format(date, DISPLAY))
        }}
        placeholder="dd/mm/aaaa"
        inputMode="numeric"
        autoComplete="off"
        disabled={disabled}
        aria-invalid={invalid || (text.trim() !== "" && !date)}
        className="pr-10"
      />
      <DatePopover
        value={date}
        onSelect={(picked) => setText(format(picked, DISPLAY))}
        disabled={disabled}
        minDate={minDate}
        maxDate={maxDate}
        defaultMonth={defaultMonth}
      />
    </div>
  )
}

type DateTimeFieldProps = DateFieldProps & {
  /** Hora usada cuando se elige una fecha y el campo de hora está vacío. */
  defaultTime?: string
}

/**
 * Fecha + hora en formato local (dd/MM/yyyy y HH:mm en 24 h), por los mismos
 * motivos que {@link DateField}. Envía `yyyy-MM-ddTHH:mm` (hora local), igual
 * que un `<input type="datetime-local">`.
 */
export function DateTimeField({
  id,
  name,
  defaultValue,
  invalid,
  disabled,
  minDate,
  maxDate,
  defaultMonth,
  defaultTime = "08:00",
}: DateTimeFieldProps) {
  const initial = splitIso(defaultValue)
  const [dateText, setDateText] = useState(initial.date)
  const [timeText, setTimeText] = useState(initial.time)

  const date = parseDateText(dateText)
  const time = parseTimeText(timeText)
  // Sin fecha no hay valor; sin hora asumimos medianoche, como el input nativo.
  const value = date ? `${format(date, ISO)}T${time ?? "00:00"}` : ""

  return (
    <div className="flex gap-2">
      <input type="hidden" name={name} value={value} />
      <div className="relative flex-1">
        <Input
          id={id ?? name}
          value={dateText}
          onChange={(e) => setDateText(e.target.value)}
          onBlur={() => {
            if (date) {
              setDateText(format(date, DISPLAY))
              if (!timeText.trim()) setTimeText(defaultTime)
            }
          }}
          placeholder="dd/mm/aaaa"
          inputMode="numeric"
          autoComplete="off"
          disabled={disabled}
          aria-invalid={invalid || (dateText.trim() !== "" && !date)}
          className="pr-10"
        />
        <DatePopover
          value={date}
          onSelect={(picked) => {
            setDateText(format(picked, DISPLAY))
            if (!timeText.trim()) setTimeText(defaultTime)
          }}
          disabled={disabled}
          minDate={minDate}
          maxDate={maxDate}
          defaultMonth={defaultMonth}
        />
      </div>
      <Input
        aria-label="Hora"
        value={timeText}
        onChange={(e) => setTimeText(e.target.value)}
        onBlur={() => {
          if (time) setTimeText(time)
        }}
        placeholder="hh:mm"
        inputMode="numeric"
        autoComplete="off"
        disabled={disabled}
        aria-invalid={timeText.trim() !== "" && !time}
        className="w-24 shrink-0 tabular-nums"
      />
    </div>
  )
}
