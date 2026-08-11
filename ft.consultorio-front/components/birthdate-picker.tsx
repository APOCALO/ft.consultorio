"use client"

import { DateField } from "@/components/date-field"

const TODAY = new Date()

/**
 * Fecha de nacimiento: {@link DateField} limitado a fechas pasadas y con el
 * calendario abriendo 25 años atrás cuando el campo está vacío.
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
  return (
    <DateField
      id={id}
      name={name}
      defaultValue={defaultValue}
      invalid={invalid}
      disabled={disabled}
      maxDate={TODAY}
      defaultMonth={new Date(TODAY.getFullYear() - 25, 0)}
    />
  )
}
