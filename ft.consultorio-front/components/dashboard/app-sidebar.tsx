"use client"

import {
  CalendarDays,
  LayoutDashboard,
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
} from "@/components/ui/sidebar"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"

type NavItem = {
  title: string
  icon: LucideIcon
  active?: boolean
}

const NAV_ITEMS: NavItem[] = [
  { title: "Panel", icon: LayoutDashboard, active: true },
  { title: "Agenda", icon: CalendarDays },
  { title: "Pacientes", icon: Users },
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

export function AppSidebar() {
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
              {NAV_ITEMS.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton
                    isActive={item.active}
                    tooltip={item.title}
                  >
                    <item.icon />
                    <span>{item.title}</span>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" tooltip="Laura Saldarriaga">
              <Avatar className="size-8 rounded-full">
                <AvatarFallback className="rounded-full bg-gradient-to-br from-bronze-300 to-bronze-600 text-xs font-semibold text-white">
                  LS
                </AvatarFallback>
              </Avatar>
              <div className="grid flex-1 text-left leading-tight group-data-[collapsible=icon]:hidden">
                <span className="truncate text-sm font-medium">
                  Laura Saldarriaga
                </span>
                <span className="truncate text-xs text-muted-foreground">
                  Fisioterapeuta
                </span>
              </div>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  )
}
