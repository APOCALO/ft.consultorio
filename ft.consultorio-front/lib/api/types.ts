/**
 * Contratos compartidos con el backend (Ft.Consultorio).
 *
 * Los handlers del API devuelven `ErrorOr<ApiResponse<T>>`; en éxito el cuerpo
 * es un {@link ApiResponse}, y en error un `ProblemDetails` (RFC 7807).
 * El API serializa en camelCase.
 */

/** Sobre estándar de respuestas exitosas del backend. */
export interface ApiResponse<T> {
  data: T
  success: boolean
  responseTime: number
  pagination?: {
    totalCount: number
    pageSize: number
    pageNumber: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
  } | null
}

/** Cuerpo de error RFC 7807 que emite `ApiBaseProblemDetailsFactory`. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  /** Errores por campo cuando la validación (FluentValidation) falla. */
  errors?: Record<string, string[]>
  [key: string]: unknown
}

/**
 * Error tipado que representa una respuesta no exitosa del backend.
 * Preserva el status HTTP y (si viene) el mapa de errores por campo.
 */
export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail || problem.title || `Request failed (${status})`)
    this.name = "ApiError"
    this.status = status
    this.problem = problem
    this.fieldErrors = problem.errors ?? {}
  }
}
