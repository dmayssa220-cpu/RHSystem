import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/** Ajoute le jeton d'accès sur chaque appel API, et tente un rafraîchissement automatique en cas de 401. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthService);
    const isAuthEndpoint = req.url.startsWith('/api/auth/');
    const isLogoutEndpoint = req.url === '/api/auth/deconnexion';

    const withToken = (request: typeof req) => {
        const token = auth.accessToken;
        return token && (!isAuthEndpoint || isLogoutEndpoint) ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;
    };

    return next(withToken(req)).pipe(
        catchError((error: unknown) => {
            if (error instanceof HttpErrorResponse && error.status === 401 && !isAuthEndpoint) {
                return auth.refresh().pipe(
                    switchMap(() => next(withToken(req))),
                    catchError((refreshError) => {
                        auth.forceLogout();
                        return throwError(() => refreshError);
                    })
                );
            }
            return throwError(() => error);
        })
    );
};
