import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const token = auth.token();

    const isAuthEndpoint = req.url.includes('/api/auth/login')
        || req.url.includes('/api/auth/register')
        || req.url.includes('/api/auth/password-reset');

    const authReq = token && !isAuthEndpoint
        ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
        : req;

    return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401 && !isAuthEndpoint && token) {
                auth.logout();
                void router.navigateByUrl('/auth/login');
            }
            return throwError(() => error);
        })
    );
};
