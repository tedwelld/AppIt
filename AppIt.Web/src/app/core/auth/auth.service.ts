import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, tap, throwError } from 'rxjs';
import { ApiService } from '../api/api.service';
import { AppItRole, AppItUser, AuthResponse, LoginRequest, RegisterRequest } from '../api/api.models';

const AUTH_STORAGE_KEY = 'appit.auth.user';
const AUTH_TOKEN_KEY = 'appit.auth.token';
const WELCOME_KEY = 'appit.auth.welcome';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly api = inject(ApiService);
    private readonly router = inject(Router);
    private readonly userState = signal<AppItUser | null>(this.loadUser());
    private readonly loadingState = signal(false);

    readonly user = computed(() => this.userState());
    readonly token = computed(() => localStorage.getItem(AUTH_TOKEN_KEY));
    readonly role = computed<AppItRole | null>(() => this.normalizeRole(this.userState()?.role));
    readonly roleName = computed<string>(() => String(this.userState()?.role ?? '').trim().toLowerCase());
    readonly displayName = computed(() => {
        const user = this.userState();
        return user ? `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || user.email : 'Guest';
    });
    readonly isAuthenticated = computed(() => !!this.userState());
    readonly isLoading = computed(() => this.loadingState());

    login(request: LoginRequest): Observable<AuthResponse> {
        this.loadingState.set(true);
        return this.api.login(request).pipe(
            tap((response) => this.saveSession(response)),
            tap(() => this.loadingState.set(false)),
            catchError((error) => {
                this.loadingState.set(false);
                return throwError(() => error);
            })
        );
    }

    register(request: RegisterRequest): Observable<AuthResponse> {
        this.loadingState.set(true);
        return this.api.register(request).pipe(
            tap((response) => this.saveSession(response)),
            tap(() => this.loadingState.set(false)),
            catchError((error) => {
                this.loadingState.set(false);
                return throwError(() => error);
            })
        );
    }

    requestPasswordReset(email: string): Observable<{ message: string; resetToken?: string }> {
        return this.api.requestPasswordReset(email).pipe(
            map((response) => ({
                message: response?.message ?? 'If the account exists, reset instructions have been issued.',
                resetToken: response?.resetToken
            }))
        );
    }

    confirmPasswordReset(email: string, token: string, newPassword: string): Observable<unknown> {
        return this.api.confirmPasswordReset(email, token, newPassword);
    }

    logout(): void {
        this.userState.set(null);
        localStorage.removeItem(AUTH_STORAGE_KEY);
        localStorage.removeItem(AUTH_TOKEN_KEY);
        localStorage.removeItem(WELCOME_KEY);
        void this.router.navigateByUrl('/auth/login');
    }

    isAdmin(): boolean {
        const role = this.role();
        return role === 'admin' || role === 'super';
    }

    navigateAfterLogin(): Promise<boolean> {
        return this.router.navigateByUrl(this.isAdmin() ? '/admin/dashboard' : '/user/dashboard');
    }

    welcomeMessage(): string {
        const value = localStorage.getItem(WELCOME_KEY) ?? '';
        localStorage.removeItem(WELCOME_KEY);
        return value;
    }

    patchUser(partial: Partial<AppItUser>): void {
        const current = this.userState();
        if (!current) return;
        const updated = { ...current, ...partial };
        this.userState.set(updated);
        localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(updated));
    }

    private saveSession(response: AuthResponse): void {
        if (!response?.user) {
            throw new Error('Invalid auth response.');
        }

        const normalizedUser = {
            ...response.user,
            role: this.normalizeRole(response.user.role) ?? 'regular'
        };

        this.userState.set(normalizedUser);
        localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(normalizedUser));
        if (response.token) {
            localStorage.setItem(AUTH_TOKEN_KEY, response.token);
        }
        localStorage.setItem(WELCOME_KEY, `Welcome ${normalizedUser.firstName ?? normalizedUser.email}`.trim());
        void this.navigateAfterLogin();
    }

    private loadUser(): AppItUser | null {
        const raw = localStorage.getItem(AUTH_STORAGE_KEY);
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw) as AppItUser;
        } catch {
            localStorage.removeItem(AUTH_STORAGE_KEY);
            localStorage.removeItem(AUTH_TOKEN_KEY);
            return null;
        }
    }

    private normalizeRole(role: unknown): AppItRole | null {
        const value = String(role ?? '').trim().toLowerCase();
        if (!value) return null;
        if (value === 'super') return 'super';
        if (value === 'regular' || value === 'customer' || value === 'guest' || value === 'user') return 'regular';
        // Every named back-office / staff role operates at the admin tier; the specific
        // role name (roleName()) is what drives finer-grained menu visibility.
        return 'admin';
    }
}
