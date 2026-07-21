import Link from "next/link"
import type { Metadata } from "next"

import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { LoginForm } from "./login-form"

export const metadata: Metadata = {
  title: "Iniciar sesión · Laura Saldarriaga",
}

export default function LoginPage() {
  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-xl">Bienvenida de vuelta</CardTitle>
        <CardDescription>
          Ingresa tus datos para acceder al consultorio.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <LoginForm />
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          ¿No tienes cuenta?{" "}
          <Link
            href="/register"
            className="font-medium text-foreground underline-offset-4 hover:underline"
          >
            Regístrate
          </Link>
        </p>
      </CardFooter>
    </Card>
  )
}
