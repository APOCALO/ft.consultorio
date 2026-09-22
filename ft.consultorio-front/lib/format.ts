/** Formateadores compartidos (locale es-CO). */

const currency = new Intl.NumberFormat("es-CO", {
  style: "currency",
  currency: "COP",
  maximumFractionDigits: 0,
})

const integer = new Intl.NumberFormat("es-CO", {
  maximumFractionDigits: 0,
})

const dateFmt = new Intl.DateTimeFormat("es-CO", {
  day: "2-digit",
  month: "short",
  year: "numeric",
})

const dateTimeFmt = new Intl.DateTimeFormat("es-CO", {
  day: "2-digit",
  month: "short",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
})

export function money(value: number): string {
  return currency.format(value ?? 0)
}

/** Dígitos de un monto: `50.000`, `$ 50.000` o `50000` → `"50000"`. */
export function copDigits(value: string | number | null | undefined): string {
  if (value == null || value === "") return ""
  return String(value).replace(/\D/g, "").replace(/^0+(?=\d)/, "")
}

/** `50000` → `50.000`. Vacío si no hay dígitos. */
export function formatCopInput(value: string | number | null | undefined): string {
  const digits = copDigits(value)
  if (!digits) return ""
  return integer.format(Number(digits))
}

/** `50.000` / `$ 50.000` / `50000` → número. `NaN` si no hay dígitos. */
export function parseCop(value: string | number | null | undefined): number {
  if (typeof value === "number") return Number.isFinite(value) ? value : Number.NaN
  const digits = copDigits(value)
  if (!digits) return Number.NaN
  return Number(digits)
}

export function formatDate(iso?: string | null): string {
  if (!iso) return "—"
  const d = new Date(iso)
  return Number.isNaN(d.getTime()) ? "—" : dateFmt.format(d)
}

export function formatDateTime(iso?: string | null): string {
  if (!iso) return "—"
  const d = new Date(iso)
  return Number.isNaN(d.getTime()) ? "—" : dateTimeFmt.format(d)
}

export function initialsFromName(fullName: string): string {
  const parts = fullName.trim().split(/\s+/)
  const first = parts[0]?.charAt(0) ?? ""
  const second = parts[1]?.charAt(0) ?? ""
  return `${first}${second}`.toUpperCase() || "?"
}
