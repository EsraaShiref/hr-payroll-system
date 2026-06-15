import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, Observable, from, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

let isRefreshing = false;
const refreshSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.accessToken();

  if (req.url.includes('/auth/')) {
    return next(req);
  }

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token) {
        return handle401Error(authReq, next, authService, router);
      }
      return throwError(() => error);
    }),
  );
};

function handle401Error(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router,
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshSubject.next(null);

    return from(authService.refreshAccessToken()).pipe(
      switchMap(newToken => {
        isRefreshing = false;
        if (newToken) {
          refreshSubject.next(newToken);
          return next(
            req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }),
          );
        }
        router.navigate(['/login']);
        return throwError(() => new Error('Session expired'));
      }),
      catchError(err => {
        isRefreshing = false;
        refreshSubject.next(null);
        router.navigate(['/login']);
        return throwError(() => err);
      }),
    );
  }

  return refreshSubject.pipe(
    filter(t => t !== null),
    take(1),
    switchMap(newToken =>
      next(
        req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }),
      ),
    ),
  );
}
