export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
  traceId?: string
  errors?: Record<string, string[]>
}

/**
 * Extracts a human-readable message from an RFC 9457 problem document.
 * Prefers `detail`, then the field validation messages, then `title`.
 */
export function problemMessage(problem: ProblemDetails | null | undefined, fallback: string): string {
  if (!problem) return fallback
  if (problem.detail) return problem.detail

  if (problem.errors) {
    const messages = Object.values(problem.errors).flat()
    if (messages.length > 0) return messages.join('; ')
  }

  return problem.title ?? fallback
}
