import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
        <section class="auth-login-page">
            <div class="auth-login-shell">
                <article class="auth-login-card">
                    <div class="auth-login-card-content">
                        <div class="auth-login-brand-row">
                            <div class="auth-login-brand">
                                <div class="auth-login-mark grid place-items-center text-white font-bold">AI</div>
                                <div>
                                    <h1 class="auth-login-brand-name">AppIt</h1>
                                    <p class="auth-login-brand-subtitle">Adventure & Hospitality Suite</p>
                                </div>
                            </div>
                        </div>

                        <h2 class="auth-login-title">Welcome Back</h2>
                        <p class="auth-login-subtitle">Sign in to continue.</p>

                        <form class="auth-login-form" (ngSubmit)="login()">
                            <div class="auth-login-fields">
                                <label class="auth-login-input-wrap">
                                    <i class="pi pi-envelope"></i>
                                    <input class="auth-login-input" type="email" name="email" [(ngModel)]="loginEmail" placeholder="Email" required />
                                </label>
                                <label class="auth-login-input-wrap">
                                    <i class="pi pi-lock"></i>
                                    <input class="auth-login-input" type="password" name="password" [(ngModel)]="loginPassword" placeholder="Password" required />
                                </label>
                            </div>
                            <button class="auth-login-submit" type="submit" [disabled]="auth.isLoading()">Sign In</button>
                        </form>

                        <p class="auth-login-bottom-note" *ngIf="status()">{{ status() }}</p>
                    </div>
                    <div class="auth-login-waves"><div class="auth-login-wave auth-login-wave-one"></div><div class="auth-login-wave auth-login-wave-two"></div><div class="auth-login-wave auth-login-wave-three"></div></div>
                </article>
            </div>
        </section>
    `
})
export class AuthPage {
    readonly auth = inject(AuthService);
    readonly status = signal('');

    loginEmail = '';
    loginPassword = '';

    login(): void {
        this.auth.login({ email: this.loginEmail.trim(), password: this.loginPassword.trim() }).subscribe({
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err?.error ?? err, 'Request failed.');
    }
}
