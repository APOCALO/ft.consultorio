"use client"

import { useState } from "react"
import { format, parse } from "date-fns"
import { es } from "date-fns/locale"
import { CalendarIcon } from "lucide-react"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"

/** Valor que el backend espera (DateOnly). */
const ISO = "yyyy-MM-dd"
/** Formato visible en español. */
const DISPLAY = "dd/MM/yyyy"

const TODAY = new Date()

/**
 * Selector de fecha de nacimiento. Un `<input type="date">` nativo muestra el
 * formato del locale del navegador (en-US → MM/dd/yyyy) y no es configurable,
 * así que usamos un date picker que muestra dd/MM/yyyy y envía yyyy-MM-dd por un
 * input oculto (sin desfase de zona horaria).
 */
export function BirthDatePicker({
  id = "birthDate",
  name = "birthDate",
  defaultValue,
  invalid,
  disabled,
}: {
  id?: string
  name?: string
  defaultValue?: string
  invalid?: boolean
  disabled?: boolean
}) {
  const [date, setDate] = useState<Date | undefined>(() =>
    defaultValue ? parse(defaultValue, ISO, new Date()) : undefined,
  )
  const [open, setOpen] = useState(false)

  return (
    <>
      <input type="hidden" name={name} value={date ? format(date, ISO) : ""} />
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger
          render={
            <Button
              id={id}
              type="button"
              variant="outline"
              disabled={disabled}
              aria-invalid={invalid}
              className={cn(
                "w-full justify-between rounded-3xl font-normal",
                !date && "text-muted-foreground",
              )}
            />
          }
        >
          {date ? format(date, DISPLAY, { locale: es }) : "dd/mm/aaaa"}
          <CalendarIcon data-icon="inline-end" className="opacity-60" />
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={date}
            onSelect={(value) => {
              setDate(value)
              setOpen(false)
            }}
            defaultMonth={date ?? new Date(TODAY.getFullYear() - 25, 0)}
            captionLayout="dropdown"
            startMonth={new Date(1920, 0)}
            endMonth={TODAY}
            disabled={{ after: TODAY }}
            locale={es}
            autoFocus
          />
        </PopoverContent>
      </Popover>
    </>
  )
}
