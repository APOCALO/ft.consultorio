"use client"

import { useState } from "react"
import { MoreHorizontal, Pencil, Trash2, UserCheck, UserPlus } from "lucide-react"

import {
  deletePatientAction,
  dischargePatientAction,
} from "@/lib/medical-records/actions"
import { PatientStatus, type Patient } from "@/lib/medical-records/types"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { ConfirmActionDialog } from "@/components/common/confirm-action-dialog"
import { PatientFormDialog } from "./patient-form-dialog"

/** Botón "Nuevo paciente" con su diálogo de creación. */
export function NewPatientButton() {
  const [open, setOpen] = useState(false)
  return (
    <>
      <Button size="sm" onClick={() => setOpen(true)}>
        <UserPlus data-icon="inline-start" />
        Nuevo paciente
      </Button>
      <PatientFormDialog open={open} onOpenChange={setOpen} />
    </>
  )
}

/** Menú de acciones por fila: editar, dar de alta, eliminar. */
export function PatientRowActions({ patient }: { patient: Patient }) {
  const [edit, setEdit] = useState(false)
  const [discharge, setDischarge] = useState(false)
  const [remove, setRemove] = useState(false)

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button variant="ghost" size="icon-sm" aria-label="Acciones">
              <MoreHorizontal />
            </Button>
          }
        />
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={() => setEdit(true)}>
            <Pencil data-icon="inline-start" />
            Editar
          </DropdownMenuItem>
          {patient.status !== PatientStatus.Discharged ? (
            <DropdownMenuItem onClick={() => setDischarge(true)}>
              <UserCheck data-icon="inline-start" />
              Dar de alta
            </DropdownMenuItem>
          ) : null}
          <DropdownMenuSeparator />
          <DropdownMenuItem variant="destructive" onClick={() => setRemove(true)}>
            <Trash2 data-icon="inline-start" />
            Eliminar
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>

      <PatientFormDialog open={edit} onOpenChange={setEdit} patient={patient} />

      <ConfirmActionDialog
        open={discharge}
        onOpenChange={setDischarge}
        action={dischargePatientAction}
        hidden={{ id: patient.id }}
        title="Dar de alta"
        description={`¿Dar de alta a ${patient.fullName}? Su estado cambiará a "Dado de alta".`}
        confirmLabel="Dar de alta"
      />

      <ConfirmActionDialog
        open={remove}
        onOpenChange={setRemove}
        action={deletePatientAction}
        hidden={{ id: patient.id }}
        title="Eliminar paciente"
        description={`Esta acción eliminará a ${patient.fullName} y no se puede deshacer.`}
        confirmLabel="Eliminar"
        destructive
      />
    </>
  )
}
