import { redirect } from "next/navigation"
import type { Metadata } from "next"

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { VerifyEmailForm } from "./verify-email-form"

export const metadata: Metadata = {
  title: "Verifica tu correo · Laura Saldarriaga",
}

export default async function VerifyEmailPage({
  searchParams,
}: {
  searchParams: Promise<{ email?: string }>
}) {
  const { email } = await searchParams

  // Sin correo en el flujo no hay nada que verificar → volver al registro.
  if (!email) {
    redirect("/register")
  }

  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-xl">Verifica tu correo</CardTitle>
        <CardDescription>
          Ingresa el código que enviamos a{" "}
          <span className="font-medium text-foreground">{email}</span>.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <VerifyEmailForm email={email} />
      </CardContent>
    </Card>
  )
}
