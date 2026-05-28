import { HttpInterceptorFn } from '@angular/common/http';

const AUTH_TOKEN_KEY = 'appit.auth.token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    if (req.url.includes('/api/auth/')) {
        return next(req);
    }

    const token = localStorage.getItem(AUTH_TOKEN_KEY);
    if (!token) {
        return next(req);
    }

    return next(req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) }));
};
