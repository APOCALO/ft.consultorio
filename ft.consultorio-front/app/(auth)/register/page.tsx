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
import { RegisterForm } from "./register-form"

export const metadata: Metadata = {
  title: "Crear cuenta · Laura Saldarriaga",
}

export default function RegisterPage() {
  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-xl">Crea tu cuenta</CardTitle>
        <CardDescription>
          Regístrate para gestionar el consultorio.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <RegisterForm />
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          ¿Ya tienes cuenta?{" "}
          <Link
            href="/login"
            className="font-medium text-foreground underline-offset-4 hover:underline"
          >
            Inicia sesión
          </Link>
        </p>
      </CardFooter>
    </Card>
  )
}
