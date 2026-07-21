import type { ReactNode } from "react"

import { ThemeToggle } from "@/components/theme-toggle"

/**
 * Layout de las pantallas de autenticación: una columna centrada, sin sidebar.
 * La marca (Laura Saldarriaga · Fisioterapeuta) encabeza el formulario.
 */
export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <main className="relative flex min-h-svh flex-col items-center justify-center overflow-hidden bg-background px-4 py-10">
      {/* Acento de marca sutil, sin competir con el formulario */}
      <div
        aria-hidden
        className="pointer-events-none absolute inset-x-0 top-0 -z-10 h-64 bg-gradient-to-b from-bronze-300/15 to-transparent"
      />

      <div className="absolute top-4 right-4">
        <ThemeToggle />
      </div>

      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <p className="font-[family-name:var(--font-cormorant)] text-2xl leading-none font-medium tracking-tight">
            Laura Saldarriaga
          </p>
          <p className="mt-1 text-xs tracking-wide text-muted-foreground uppercase">
            Fisioterapia
          </p>
        </div>

        {children}
      </div>
    </main>
  )
}
