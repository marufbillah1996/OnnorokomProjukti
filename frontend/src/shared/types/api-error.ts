/**
 * Shape of every error response from the backend — an RFC 7807 ProblemDetails object,
 * optionally carrying field-level validation errors. Every API call in the app should be
 * able to parse errors against this one shape (see shared/lib/api-client.ts).
 */
export interface ApiProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  readonly status: number;
  readonly errors?: Record<string, string[]>;

  constructor(problem: ApiProblemDetails) {
    super(problem.detail || problem.title);
    this.name = "ApiError";
    this.status = problem.status;
    this.errors = problem.errors;
  }

  /** First field-level validation message, if any — handy for a quick toast summary. */
  get firstFieldError(): string | undefined {
    if (!this.errors) return undefined;
    const firstKey = Object.keys(this.errors)[0];
    return firstKey ? this.errors[firstKey][0] : undefined;
  }
}
