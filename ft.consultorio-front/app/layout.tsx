import type { Metadata } from "next"
import { cookies } from "next/headers"
import { Inter, Cormorant_Garamond, JetBrains_Mono } from "next/font/google"

import "./globals.css"
import { TooltipProvider } from "@/components/ui/tooltip"
import { Toaster } from "@/components/ui/sonner"
import { cn } from "@/lib/utils"

// Voz por defecto de la interfaz — Inter
const inter = Inter({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-inter",
})

// Serif de marca — Cormorant Garamond (títulos y lemas)
const cormorant = Cormorant_Garamond({
  subsets: ["latin"],
  weight: ["400", "500", "600"],
  style: ["normal", "italic"],
  variable: "--font-cormorant",
})

// Datos y código — JetBrains Mono
const jetbrainsMono = JetBrains_Mono({
  subsets: ["latin"],
  weight: ["400", "500"],
  variable: "--font-jetbrains",
})

export const metadata: Metadata = {
  title: "Laura Saldarriaga · Fisioterapeuta",
  description:
    "Gestión clínica de fisioterapia. Tu bienestar es lo más importante.",
}

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  // Tema por defecto `light`; sólo oscuro si la cookie lo pide. Al fijar la clase
  // en el SSR no hay parpadeo ni <script> anti-flash.
  const isDark = (await cookies()).get("theme")?.value === "dark"

  return (
    <html
      lang="es"
      style={{ colorScheme: isDark ? "dark" : "light" }}
      className={cn(
        "antialiased",
        isDark && "dark",
        inter.variable,
        cormorant.variable,
        jetbrainsMono.variable,
      )}
    >
      <body>
        <TooltipProvider delay={300}>{children}</TooltipProvider>
        <Toaster position="top-center" richColors />
      </body>
    </html>
  )
}
