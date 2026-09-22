import Link from "next/link"
import { redirect } from "next/navigation"
import {
  Activity,
  AlertTriangle,
  ArrowRight,
  CalendarClock,
  ShieldAlert,
  TrendingUp,
  UserPlus,
  Users,
} from "lucide-react"

import { ApiError } from "@/lib/api/types"
import { getCurrentUser } from "@/lib/auth/session"
import { getDashboard, getPatients } from "@/lib/medical-records/api"
import { PATIENT_STATUS_LABELS, type Dashboard, type Patient } from "@/lib/medical-records/types"
import { formatDate, initialsFromName, money } from "@/lib/format"
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

export default async function DashboardPage() {
  const user = await getCurrentUser()
  if (!user) redirect("/login")

  const sidebarUser: SidebarUser = {
    fullName: user.fullName,
    roleLabel: user.roles[0]?.name ?? "Usuario",
    initials: initialsFrom(user.firstName, user.lastName),
  }

  let dashboard: Dashboard | null = null
  let recent: Patient[] = []
  let forbidden = false
  try {
    ;[dashboard, recent] = await Promise.all([
      getDashboard(),
      getPatients({ pageSize: 5 }).then((r) => r.items),
    ])
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) redirect("/logout")
    if (error instanceof ApiError && error.status === 403) forbidden = true
    else throw error
  }

  return (
    <SidebarProvider>
      <AppSidebar user={sidebarUser} />
      <SidebarInset>
        <header className="sticky top-0 z-10 flex flex-wrap items-center gap-x-4 gap-y-2 border-b border-border bg-background/70 px-4 py-3 backdrop-blur-md sm:px-6">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator orientation="vertical" className="mr-1 hidden h-6 sm:block" />
            <div>
              <h1 className="text-lg leading-tight font-semibold tracking-tight">
                {greeting()}, {user.firstName}
              </h1>
              <p className="text-xs text-muted-foreground">
                Resumen del consultorio
              </p>
            </div>
          </div>

          <div className="ml-auto flex items-center gap-2">
            <Button size="sm" render={<Link href="/patients" />}>
              <UserPlus data-icon="inline-start" />
              Pacientes
            </Button>
          </div>
        </header>

        <div className="flex flex-1 flex-col gap-4 p-4 sm:p-6">
          {forbidden ? (
            <AdminNotice />
          ) : dashboard ? (
            <>
              <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <Kpi
                  icon={<CalendarClock className="size-3.5" />}
                  label="Sesiones hoy"
                  value={String(dashboard.sessionsToday)}
                />
                <Kpi
                  icon={<Users className="size-3.5" />}
                  label="Pacientes activos"
                  value={String(dashboard.activePatients)}
                  hint={`${dashboard.patients} en total`}
                />
                <Kpi
                  icon={<TrendingUp className="size-3.5" />}
                  label="Ingresos del mes"
                  value={money(dashboard.incomeThisMonth)}
                  action={{ href: "/ingresos", label: "Ver quién pagó" }}
                />
                <Kpi
                  icon={<AlertTriangle className="size-3.5" />}
                  label="Saldo pendiente"
                  value={money(dashboard.pendingBalance)}
                  action={{ href: "/saldo", label: "Ver quién debe" }}
                />
              </section>

              <section className="grid gap-4">
                <Card>
                  <CardHeader className="flex-row items-center justify-between">
                    <CardTitle className="text-sm">Pacientes recientes</CardTitle>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-muted-foreground"
                      render={<Link href="/patients" />}
                    >
                      Ver todos
                      <ArrowRight data-icon="inline-end" />
                    </Button>
                  </CardHeader>
                  <CardContent className="flex flex-col">
                    {recent.length === 0 ? (
                      <p className="py-6 text-center text-sm text-muted-foreground">
                        Aún no hay pacientes registrados.
                      </p>
                    ) : (
                      recent.map((p, index) => (
                        <div key={p.id}>
                          {index > 0 ? <Separator /> : null}
                          <Link
                            href={`/patients/${p.id}`}
                            className="flex items-center gap-3 rounded-md py-2.5 transition-colors hover:bg-muted/50"
                          >
                            <Avatar className="size-8">
                              <AvatarFallback className="bg-gradient-to-br from-bronze-300 to-bronze-600 text-[11px] font-semibold text-white">
                                {initialsFromName(p.fullName)}
                              </AvatarFallback>
                            </Avatar>
                            <div className="min-w-0">
                              <p className="truncate text-sm font-medium">
                                {p.fullName}
                              </p>
                              <p className="truncate text-xs text-muted-foreground">
                                Doc. {p.document} · {formatDate(p.createdAt)}
                              </p>
                            </div>
                            <Badge variant="outline" className="ml-auto">
                              {PATIENT_STATUS_LABELS[p.status]}
                            </Badge>
                          </Link>
                        </div>
                      ))
                    )}
                  </CardContent>
                </Card>
              </section>
            </>
          ) : null}
        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}

function Kpi({
  icon,
  label,
  value,
  hint,
  action,
  tone = "muted",
}: {
  icon: React.ReactNode
  label: string
  value: string
  hint?: string
  action?: { href: string; label: string }
  tone?: "success" | "warning" | "muted"
}) {
  const toneClass = {
    success: "text-success",
    warning: "text-warning",
    muted: "text-muted-foreground",
  }[tone]

  return (
    <Card>
      <CardHeader>
        <CardDescription className="flex items-center gap-1.5">
          {icon}
          {label}
        </CardDescription>
        <CardTitle className="text-[26px] tracking-tight tabular-nums">
          {value}
        </CardTitle>
      </CardHeader>
      {hint ? (
        <CardContent>
          <p className={`text-[11.5px] ${toneClass}`}>{hint}</p>
        </CardContent>
      ) : null}
      {action ? (
        <CardContent>
          <Button
            variant="ghost"
            size="sm"
            className="-ml-2 text-muted-foreground"
            render={<Link href={action.href} />}
          >
            {action.label}
            <ArrowRight data-icon="inline-end" />
          </Button>
        </CardContent>
      ) : null}
    </Card>
  )
}

function AdminNotice() {
  return (
    <Card className="mx-auto max-w-lg">
      <CardHeader className="items-center text-center">
        <div className="mb-2 flex size-11 items-center justify-center rounded-full bg-warning-soft text-warning-soft-foreground">
          <ShieldAlert className="size-5" />
        </div>
        <CardTitle className="text-base">Acceso restringido</CardTitle>
        <CardDescription>
          Tu cuenta no tiene el rol <span className="font-medium">Admin</span>,
          necesario para ver el consultorio. Un administrador debe asignártelo
          en la base de datos.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex items-center justify-center gap-2 text-xs text-muted-foreground">
        <Activity className="size-3.5" />
        Vuelve a iniciar sesión una vez asignado el rol.
      </CardContent>
    </Card>
  )
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
