"use client"

import { useEffect, useRef, useState, useTransition } from "react"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { Search } from "lucide-react"

import { Input } from "@/components/ui/input"
import { Spinner } from "@/components/ui/spinner"

/** Búsqueda por nombre o documento; actualiza `?search=` con debounce. */
export function PatientsSearch() {
  const router = useRouter()
  const pathname = usePathname()
  const params = useSearchParams()
  const [value, setValue] = useState(params.get("search") ?? "")
  const [isPending, startTransition] = useTransition()
  const first = useRef(true)

  useEffect(() => {
    if (first.current) {
      first.current = false
      return
    }
    const t = setTimeout(() => {
      const next = new URLSearchParams(params)
      if (value) next.set("search", value)
      else next.delete("search")
      next.delete("page")
      startTransition(() => router.replace(`${pathname}?${next.toString()}`))
    }, 350)
    return () => clearTimeout(t)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [value])

  return (
    <div className="relative w-full max-w-xs">
      <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        value={value}
        onChange={(e) => setValue(e.target.value)}
        placeholder="Buscar por nombre o documento"
        className="pl-9"
      />
      {isPending ? (
        <Spinner className="absolute top-1/2 right-3 size-4 -translate-y-1/2" />
      ) : null}
    </div>
  )
}
