"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import {
  CalendarDays,
  LayoutDashboard,
  LogOut,
  Settings,
  Users,
  Wallet,
  type LucideIcon,
} from "lucide-react"

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
} from "@/components/ui/sidebar"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { ThemeToggle } from "@/components/theme-toggle"
import { logoutAction } from "@/lib/auth/actions"

export type SidebarUser = {
  fullName: string
  roleLabel: string
  initials: string
}

type NavItem = {
  title: string
  icon: LucideIcon
  href?: string
}

const NAV_ITEMS: NavItem[] = [
  { title: "Panel", icon: LayoutDashboard, href: "/" },
  { title: "Pacientes", icon: Users, href: "/patients" },
  { title: "Agenda", icon: CalendarDays },
  { title: "Caja", icon: Wallet },
  { title: "Ajustes", icon: Settings },
]

// Marca de Laura Saldarriaga — trazos en cobre (guiño al logo del brand book).
function BrandMark() {
  return (
    <svg
      viewBox="0 0 64 64"
      fill="none"
      aria-hidden
      className="size-6 shrink-0"
    >
      <circle cx="35" cy="19" r="4.4" fill="var(--primary)" />
      <path
        d="M31 25C31 34 27 40 22 46M35 25C35 33 39 39 44 44"
        stroke="var(--primary)"
        strokeWidth="3.4"
        strokeLinecap="round"
      />
      <path
        d="M13 34C22 30 30 31 33 37C36 31 44 30 52 35"
        stroke="var(--bronze-600)"
        strokeWidth="3.4"
        strokeLinecap="round"
      />
    </svg>
  )
}

export function AppSidebar({ user }: { user: SidebarUser }) {
  const pathname = usePathname()

  function isActive(href?: string) {
    if (!href) return false
    if (href === "/") return pathname === "/"
    return pathname === href || pathname.startsWith(`${href}/`)
  }

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <div className="flex items-center gap-2 px-2 py-1.5 group-data-[collapsible=icon]:px-0">
          <BrandMark />
          <span className="font-serif text-base font-semibold tracking-tight group-data-[collapsible=icon]:hidden">
            Laura S.
          </span>
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {NAV_ITEMS.map((item) =>
                item.href ? (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton
                      isActive={isActive(item.href)}
                      tooltip={item.title}
                      render={
                        <Link href={item.href}>
                          <item.icon />
                          <span>{item.title}</span>
                        </Link>
                      }
                    />
                  </SidebarMenuItem>
                ) : (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton
                      tooltip={`${item.title} · próximamente`}
                      disabled
                    >
                      <item.icon />
                      <span>{item.title}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ),
              )}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <SidebarMenu>
          {/* Identidad del usuario (informativo) */}
          <SidebarMenuItem>
            <div className="flex items-center gap-2 rounded-xl px-2 py-1.5 group-data-[collapsible=icon]:justify-center group-data-[collapsible=icon]:px-0">
              <Avatar className="size-8 shrink-0 rounded-full">
                <AvatarFallback className="rounded-full bg-gradient-to-br from-bronze-300 to-bronze-600 text-xs font-semibold text-white">
                  {user.initials}
                </AvatarFallback>
              </Avatar>
              <div className="grid flex-1 text-left leading-tight group-data-[collapsible=icon]:hidden">
                <span className="truncate text-sm font-medium">
                  {user.fullName}
                </span>
                <span className="truncate text-xs text-muted-foreground">
                  {user.roleLabel}
                </span>
              </div>
            </div>
          </SidebarMenuItem>

          <SidebarSeparator className="my-1" />

          {/* Cambiar tema */}
          <SidebarMenuItem>
            <div className="flex items-center justify-between rounded-xl px-3 py-1 group-data-[collapsible=icon]:justify-center group-data-[collapsible=icon]:px-0">
              <span className="text-sm text-muted-foreground group-data-[collapsible=icon]:hidden">
                Tema
              </span>
              <ThemeToggle />
            </div>
          </SidebarMenuItem>

          {/* Cerrar sesión */}
          <SidebarMenuItem>
            <form action={logoutAction} className="w-full">
              <SidebarMenuButton
                type="submit"
                tooltip="Cerrar sesión"
                className="text-muted-foreground hover:text-foreground"
              >
                <LogOut />
                <span>Cerrar sesión</span>
              </SidebarMenuButton>
            </form>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  )
}
