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

export default async function SaldoPage() {
  const user = await getCurrentUser()
  if (!user) redirect("/login")

  let total = 0
  let groups: Awaited<ReturnType<typeof getDashboard>>["pending"]
  let forbidden = false
  try {
    const dashboard = await getDashboard()
    total = dashboard.pendingBalance
    groups = dashboard.pending
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) redirect("/logout")
    if (error instanceof ApiError && error.status === 403) forbidden = true
    else throw error
  }

  return (
    <AppShell
      user={user}
      title="Saldo pendiente"
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
          Tu cuenta necesita el rol Admin para ver el saldo pendiente.
        </p>
      ) : (
        <SessionBalanceList
          groups={groups}
          empty="Nadie tiene saldo pendiente."
        />
      )}
    </AppShell>
  )
}
