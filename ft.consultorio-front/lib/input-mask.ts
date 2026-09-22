/** Máscaras de escritura para fecha (dd/mm/aaaa) y hora (hh:mm). */

export function isDeleteInput(event: Event): boolean {
  if (!("inputType" in event)) return false
  const inputType = (event as InputEvent).inputType
  return typeof inputType === "string" && inputType.startsWith("delete")
}

/**
 * Posición del cursor después de enmascarar, contando solo dígitos.
 * Si el cursor iba al final, queda al final (incluye el separador recién puesto).
 */
export function caretForMasked(
  raw: string,
  selectionStart: number,
  masked: string,
): number {
  const digitsBefore = raw.slice(0, selectionStart).replace(/\D/g, "").length
  const totalDigits = masked.replace(/\D/g, "").length
  if (digitsBefore >= totalDigits) return masked.length

  let seen = 0
  for (let i = 0; i < masked.length; i++) {
    const code = masked.charCodeAt(i)
    if (code >= 48 && code <= 57) {
      seen++
      if (seen === digitsBefore) return i + 1
    }
  }
  return masked.length
}

/**
 * Inserta `/` al escribir: `23111994` → `23/11/1994`.
 * Si la persona escribe las barras (`3/5/2024`), se respetan.
 * Guion o punto (`2024-05-03`, `23.11.1994`) se dejan para normalizar al salir.
 */
export function maskDate(next: string, deleting = false): string {
  if (/[-.]/.test(next)) {
    return next.replace(/[^\d./-]/g, "").slice(0, 10)
  }

  const raw = next.replace(/[^\d/]/g, "").replace(/\/{2,}/g, "/")
  const segments = raw.split("/")

  let day = segments[0] ?? ""
  let month = segments[1] ?? ""
  let year = segments.slice(2).join("")

  if (day.length > 2) {
    month = day.slice(2) + month
    day = day.slice(0, 2)
  }
  if (month.length > 2) {
    year = month.slice(2) + year
    month = month.slice(0, 2)
  }
  year = year.slice(0, 4)

  const hadMonth = segments.length > 1
  const hadYear = segments.length > 2

  let out = day

  if (hadMonth || month) {
    out += `/${month}`
  } else if (day.length === 2 && !deleting) {
    out += "/"
  }

  if (hadYear || year) {
    out += `/${year}`
  } else if (month.length === 2 && !deleting && out.includes("/")) {
    out += "/"
  }

  return out.startsWith("/") ? out.slice(1) : out
}

/** Inserta `:` al escribir: `0905` → `09:05`. */
export function maskTime(next: string, deleting = false): string {
  const raw = next.replace(/[^\d:]/g, "").replace(/:{2,}/g, ":")
  const segments = raw.split(":")

  let hour = segments[0] ?? ""
  let minute = segments.slice(1).join("")

  if (hour.length > 2) {
    minute = hour.slice(2) + minute
    hour = hour.slice(0, 2)
  }
  minute = minute.slice(0, 2)

  const hadMinute = segments.length > 1
  let out = hour
  if (hadMinute || minute) {
    out += `:${minute}`
  } else if (hour.length === 2 && !deleting) {
    out += ":"
  }
  return out
}
