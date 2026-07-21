import { redirect } from "next/navigation"
import { Bell, CalendarDays, Plus, Search } from "lucide-react"

import { getCurrentUser } from "@/lib/auth/session"
import { AppSidebar, type SidebarUser } from "@/components/dashboard/app-sidebar"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar"

type Kpi = {
  label: string
  value: string
  trend: string
  tone: "success" | "warning" | "muted"
}

const KPIS: Kpi[] = [
  { label: "Citas hoy", value: "6", trend: "▲ 2 vs. ayer", tone: "success" },
  {
    label: "Pacientes activos",
    value: "48",
    trend: "3 nuevos esta semana",
    tone: "muted",
  },
  {
    label: "Caja del día",
    value: "$ 640.000",
    trend: "1 copago pendiente",
    tone: "warning",
  },
]

const TREND_TONE = {
  success: "text-success",
  warning: "text-warning",
  muted: "text-muted-foreground",
} as const

type Appointment = {
  time: string
  initials: string
  name: string
  service: string
  gradient: string
  status?: { label: string; variant: "success" | "warning" }
}

const AGENDA: Appointment[] = [
  {
    time: "09:00",
    initials: "MR",
    name: "M. Restrepo",
    service: "Descarga",
    gradient: "from-bronze-300 to-bronze-600",
    status: { label: "Confirmada", variant: "success" },
  },
  {
    time: "10:00",
    initials: "JT",
    name: "J. Tobón",
    service: "Ventosas",
    gradient: "from-plum-300 to-plum-600",
    status: { label: "Por llegar", variant: "warning" },
  },
  {
    time: "11:30",
    initials: "LC",
    name: "L. Cano",
    service: "Valoración",
    gradient: "from-[var(--info-500)] to-[var(--info-700)]",
  },
]

type AlertItem = {
  text: string
  detail?: string
  variant: "warning" | "info"
}

const ALERTS: AlertItem[] = [
  { text: "Copago pendiente", detail: "J. Tobón", variant: "warning" },
  { text: "2 valoraciones sin firmar", variant: "info" },
]

const ALERT_TONE = {
  warning:
    "border-warning-soft-border bg-warning-soft text-warning-soft-foreground",
  info: "border-info-soft-border bg-info-soft text-info-soft-foreground",
} as const

export default async function DashboardPage() {
  const user = await getCurrentUser()
  if (!user) redirect("/login")

  const sidebarUser: SidebarUser = {
    fullName: user.fullName,
    roleLabel: user.roles[0]?.name ?? "Usuario",
    initials: initialsFrom(user.firstName, user.lastName),
  }

  return (
    <SidebarProvider>
      <AppSidebar user={sidebarUser} />
      <SidebarInset>
        {/* Header sticky con blur · saludo contextual + un solo CTA primario */}
        <header className="sticky top-0 z-10 flex flex-wrap items-center gap-x-4 gap-y-2 border-b border-border bg-background/70 px-4 py-3 backdrop-blur-md sm:px-6">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator
              orientation="vertical"
              className="mr-1 hidden h-6 sm:block"
            />
            <div>
              <h1 className="text-lg leading-tight font-semibold tracking-tight">
                {greeting()}, {user.firstName}
              </h1>
              <p className="text-xs text-muted-foreground">
                Martes 21 de julio · 6 citas hoy
              </p>
            </div>
          </div>

          <div className="ml-auto flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              className="text-muted-foreground"
            >
              <Search data-icon="inline-start" />
              <span className="hidden sm:inline">Buscar</span>
              <kbd className="ml-1 hidden rounded-sm bg-muted px-1.5 font-mono text-[11px] sm:inline">
                ⌘K
              </kbd>
            </Button>
            <Button variant="ghost" size="icon-sm" aria-label="Notificaciones">
              <Bell />
            </Button>
            <Button size="sm">
              <Plus data-icon="inline-start" />
              Nueva cita
            </Button>
          </div>
        </header>

        <div className="flex flex-1 flex-col gap-4 p-4 sm:p-6">
          {/* KPIs · máximo 3–4, sin rellenos de color */}
          <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {KPIS.map((kpi) => (
              <Card key={kpi.label}>
                <CardHeader>
                  <CardDescription className="flex items-center gap-1.5">
                    <CalendarDays className="size-3.5" />
                    {kpi.label}
                  </CardDescription>
                  <CardTitle className="text-[26px] tracking-tight tabular-nums">
                    {kpi.value}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <p className={cnTone(kpi.tone)}>{kpi.trend}</p>
                </CardContent>
              </Card>
            ))}
          </section>

          {/* Módulos · jerarquía por tamaño, no por color */}
          <section className="grid gap-4 lg:grid-cols-[1.2fr_1fr]">
            {/* Agenda de hoy */}
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Agenda de hoy</CardTitle>
              </CardHeader>
              <CardContent className="flex flex-col">
                {AGENDA.map((appt, index) => (
                  <div key={appt.time}>
                    {index > 0 ? <Separator /> : null}
                    <div className="flex items-center gap-3 py-2.5">
                      <span className="w-11 font-mono text-[11px] text-bronze-600">
                        {appt.time}
                      </span>
                      <Avatar className="size-7">
                        <AvatarFallback
                          className={`bg-gradient-to-br ${appt.gradient} text-[10px] font-semibold text-white`}
                        >
                          {appt.initials}
                        </AvatarFallback>
                      </Avatar>
                      <span className="text-sm">
                        {appt.name}{" "}
                        <span className="text-muted-foreground">
                          · {appt.service}
                        </span>
                      </span>
                      {appt.status ? (
                        <Badge
                          variant={appt.status.variant}
                          className="ml-auto"
                        >
                          {appt.status.label}
                        </Badge>
                      ) : null}
                    </div>
                  </div>
                ))}
              </CardContent>
            </Card>

            {/* Alertas */}
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Alertas</CardTitle>
              </CardHeader>
              <CardContent className="flex flex-col gap-2">
                {ALERTS.map((alert) => (
                  <div
                    key={alert.text}
                    className={`flex items-start gap-2 rounded-md border px-3 py-2.5 text-xs ${ALERT_TONE[alert.variant]}`}
                  >
                    <Bell className="mt-0.5 size-3.5 shrink-0" />
                    <span>
                      {alert.text}
                      {alert.detail ? ` · ${alert.detail}` : ""}
                    </span>
                  </div>
                ))}
              </CardContent>
            </Card>
          </section>

        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}

function cnTone(tone: Kpi["tone"]) {
  return `flex items-center gap-1 text-[11.5px] ${TREND_TONE[tone]}`
}

function initialsFrom(firstName: string, lastName: string) {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase()
}

function greeting() {
  const hour = new Date().getHours()
  if (hour < 12) return "Buenos días"
  if (hour < 19) return "Buenas tardes"
  return "Buenas noches"
}
