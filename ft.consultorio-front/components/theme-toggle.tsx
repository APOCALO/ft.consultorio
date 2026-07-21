"use client"

import { Moon, Sun } from "lucide-react"

import { Button } from "@/components/ui/button"

/**
 * Alterna el tema claro/oscuro. El estado vive en la clase `.dark` de <html>
 * (fijada en SSR desde la cookie `theme`), así que el icono correcto se muestra
 * con utilidades `dark:*` sin necesidad de estado en React → cero mismatch de
 * hidratación. El clic alterna la clase al instante y persiste la cookie para
 * que el próximo SSR ya sirva el tema elegido (sin parpadeo).
 */
export function ThemeToggle({ className }: { className?: string }) {
  function toggle() {
    const el = document.documentElement
    const dark = !el.classList.contains("dark")
    el.classList.toggle("dark", dark)
    el.style.colorScheme = dark ? "dark" : "light"
    document.cookie = `theme=${dark ? "dark" : "light"}; path=/; max-age=31536000; samesite=lax`
  }

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon-sm"
      onClick={toggle}
      aria-label="Cambiar tema"
      title="Cambiar tema"
      className={className}
    >
      <Sun className="hidden dark:block" />
      <Moon className="block dark:hidden" />
    </Button>
  )
}
