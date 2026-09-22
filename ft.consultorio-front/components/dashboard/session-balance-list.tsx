import Link from "next/link"

import { formatDate, formatDateTime, initialsFromName, money } from "@/lib/format"
import type { DashboardPatientBalance } from "@/lib/medical-records/types"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Card, CardContent } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"

export function SessionBalanceList({
  groups,
  empty,
  showPaidAt = false,
}: {
  groups?: DashboardPatientBalance[]
  empty: string
  showPaidAt?: boolean
}) {
  if (!Array.isArray(groups)) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-sm text-muted-foreground">
          Reinicia el API de historias clínicas para ver el detalle.
        </CardContent>
      </Card>
    )
  }

  if (groups.length === 0) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-sm text-muted-foreground">
          {empty}
        </CardContent>
      </Card>
    )
  }

  const sessions = groups.reduce((sum, group) => sum + group.sessions.length, 0)

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-3">
      <p className="text-sm text-muted-foreground">
        {count(groups.length, "paciente", "pacientes")} · {count(sessions, "sesión", "sesiones")}
      </p>
      <Card>
        <CardContent className="flex flex-col">
          {groups.map((group, index) => (
            <div key={group.patientId}>
              {index > 0 ? <Separator /> : null}
              <PatientGroup group={group} showPaidAt={showPaidAt} />
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  )
}

function PatientGroup({
  group,
  showPaidAt,
}: {
  group: DashboardPatientBalance
  showPaidAt: boolean
}) {
  const href = `/patients/${group.patientId}?tab=sessions`

  return (
    <div className="flex flex-col gap-1 py-3">
      <div className="flex items-center gap-3">
        <Avatar className="size-8">
          <AvatarFallback className="bg-gradient-to-br from-bronze-300 to-bronze-600 text-[11px] font-semibold text-white">
            {initialsFromName(group.patientName)}
          </AvatarFallback>
        </Avatar>
        <div className="min-w-0 flex-1">
          <Link href={href} className="block truncate text-sm font-medium hover:underline">
            {group.patientName}
          </Link>
          <p className="text-xs text-muted-foreground">
            {count(group.sessions.length, "sesión", "sesiones")}
          </p>
        </div>
        <p className="text-sm font-medium tabular-nums">{money(group.total)}</p>
      </div>
      <ul className="flex flex-col pl-11">
        {group.sessions.map((session) => (
          <li key={session.sessionId}>
            <Link
              href={`${href}#session-${session.sessionId}`}
              className="-mx-2 flex items-baseline justify-between gap-3 rounded-md px-2 py-1.5 hover:bg-muted/50"
            >
              <span className="min-w-0">
                <span className="block text-xs">{formatDateTime(session.date)}</span>
                {showPaidAt && session.paidAt ? (
                  <span className="block text-[11px] text-muted-foreground">
                    Pagada {formatDate(session.paidAt)}
                  </span>
                ) : null}
              </span>
              <span className="shrink-0 text-xs text-muted-foreground tabular-nums">
                {money(session.price)}
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  )
}

function count(value: number, one: string, many: string) {
  return `${value} ${value === 1 ? one : many}`
}
