import { HttpErrorResponse } from '@angular/common/http';

export interface ApiResponse<T> {
  readonly data: T;
  readonly meta: ApiResponseMeta;
}

export interface ApiResponseMeta {
  readonly correlationId: string;
  readonly pagination?: PaginationMetadata;
}

export interface PaginationMetadata {
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface ApiProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly detail?: string;
  readonly instance?: string;
  readonly code?: string;
  readonly traceId?: string;
  readonly correlationId?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}

export function apiProblemMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse) || !isProblemDetails(error.error)) return fallback;
  const validationMessage = error.error.errors
    ? Object.values(error.error.errors).flat().find(message => message.trim().length > 0)
    : undefined;
  return validationMessage ?? error.error.detail ?? error.error.title ?? fallback;
}

function isProblemDetails(value: unknown): value is ApiProblemDetails {
  return typeof value === 'object' && value !== null;
}
