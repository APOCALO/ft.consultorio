import type { Metadata } from "next"
import { Inter, Cormorant_Garamond, JetBrains_Mono } from "next/font/google"

import "./globals.css"
import { ThemeProvider } from "@/components/theme-provider"
import { TooltipProvider } from "@/components/ui/tooltip"
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

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html
      lang="es"
      suppressHydrationWarning
      className={cn(
        "antialiased",
        inter.variable,
        cormorant.variable,
        jetbrainsMono.variable,
      )}
    >
      <body>
        <ThemeProvider>
          <TooltipProvider delay={300}>{children}</TooltipProvider>
        </ThemeProvider>
      </body>
    </html>
  )
}
