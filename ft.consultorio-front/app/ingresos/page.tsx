import Link from "next/link"
import { redirect } from "next/navigation"
import { ArrowLeft } from "lucide-react"

import { ApiError } from "@/lib/api/types"
import { getCurrentUser } from "@/lib/auth/session"
import { getDashboard } from "@/lib/medical-records/api"
import { money } from "@/lib/format"
import { SessionBalanceList } from "@/components/dashboard/session-balance-list"
import { AppShell } from "@/components/dashboard/app-shell"
import { Button } from "@/components/ui/button"

export default async function IngresosPage() {
  const user = await getCurrentUser()
  if (!user) redirect("/login")

  let total = 0
  let groups: Awaited<ReturnType<typeof getDashboard>>["income"]
  let forbidden = false
  try {
    const dashboard = await getDashboard()
    total = dashboard.incomeThisMonth
    groups = dashboard.income
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) redirect("/logout")
    if (error instanceof ApiError && error.status === 403) forbidden = true
    else throw error
  }

  return (
    <AppShell
      user={user}
      title="Ingresos del mes"
      subtitle={forbidden ? "Acceso restringido" : money(total)}
      actions={
        <Button variant="ghost" size="sm" className="text-muted-foreground" render={<Link href="/" />}>
          <ArrowLeft data-icon="inline-start" />
          Inicio
        </Button>
      }
    >
      {forbidden ? (
        <p className="text-sm text-muted-foreground">
          Tu cuenta necesita el rol Admin para ver los ingresos.
        </p>
      ) : (
        <SessionBalanceList
          groups={groups}
          empty="Este mes todavía no hay sesiones pagadas."
          showPaidAt
        />
      )}
    </AppShell>
  )
}
