import type { ReactNode } from "react"

import type { SessionUser } from "@/lib/auth/types"
import { AppSidebar, type SidebarUser } from "@/components/dashboard/app-sidebar"
import { Separator } from "@/components/ui/separator"
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar"

function initialsFrom(firstName: string, lastName: string) {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase()
}

/**
 * Marco común de las pantallas internas: sidebar + header sticky.
 * `title`/`subtitle` describen la página; `actions` va alineado a la derecha.
 */
export function AppShell({
  user,
  title,
  subtitle,
  actions,
  children,
}: {
  user: SessionUser
  title: ReactNode
  subtitle?: ReactNode
  actions?: ReactNode
  children: ReactNode
}) {
  const sidebarUser: SidebarUser = {
    fullName: user.fullName,
    roleLabel: user.roles[0]?.name ?? "Usuario",
    initials: initialsFrom(user.firstName, user.lastName),
  }

  return (
    <SidebarProvider>
      <AppSidebar user={sidebarUser} />
      <SidebarInset>
        <header className="sticky top-0 z-10 flex flex-wrap items-center gap-x-4 gap-y-2 border-b border-border bg-background/70 px-4 py-3 backdrop-blur-md sm:px-6">
          <div className="flex min-w-0 items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator orientation="vertical" className="mr-1 hidden h-6 sm:block" />
            <div className="min-w-0">
              <h1 className="truncate text-lg leading-tight font-semibold tracking-tight">
                {title}
              </h1>
              {subtitle ? (
                <p className="truncate text-xs text-muted-foreground">{subtitle}</p>
              ) : null}
            </div>
          </div>
          {actions ? <div className="ml-auto flex items-center gap-2">{actions}</div> : null}
        </header>
        <div className="flex flex-1 flex-col gap-4 p-4 sm:p-6">{children}</div>
      </SidebarInset>
    </SidebarProvider>
  )
}
