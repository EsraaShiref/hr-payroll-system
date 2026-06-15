import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const problem = error.error as ProblemDetails | undefined;

      if (error.status === 401) {
        router.navigate(['/login']);
        return throwError(() => error);
      }

      if (problem?.detail) {
        console.warn(`[API Error ${problem.status}] ${problem.title}: ${problem.detail}`);
      }

      return throwError(() => error);
    }),
  );
};
