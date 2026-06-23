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
                                <img class="auth-login-mark" src="assets/images/tdz-logo.png" alt="TDZ" />
                                <div>
                                    <h1 class="auth-login-brand-name">AppIt</h1>
                                    <p class="auth-login-brand-subtitle">Adventure & Hospitality Suite</p>
                                </div>
                            </div>
                        </div>

                        <div class="flex gap-3 mb-4">
                            <button type="button" class="auth-login-submit !py-2" [class.opacity-60]="mode() !== 'login'" (click)="mode.set('login')">Sign In</button>
                            <button type="button" class="auth-login-submit !py-2" [class.opacity-60]="mode() !== 'reset'" (click)="mode.set('reset')">Forgot Password</button>
                        </div>

                        <ng-container *ngIf="mode() === 'login'">
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
                        </ng-container>

                        <ng-container *ngIf="mode() === 'reset'">
                            <h2 class="auth-login-title">Reset Password</h2>
                            <p class="auth-login-subtitle">We will email reset instructions when the account exists.</p>
                            <form class="auth-login-form" (ngSubmit)="requestReset()">
                                <div class="auth-login-fields">
                                    <label class="auth-login-input-wrap">
                                        <i class="pi pi-envelope"></i>
                                        <input class="auth-login-input" type="email" name="resetEmail" [(ngModel)]="resetEmail" placeholder="Email" required />
                                    </label>
                                    <label class="auth-login-input-wrap" *ngIf="resetToken">
                                        <i class="pi pi-key"></i>
                                        <input class="auth-login-input" type="text" name="resetToken" [(ngModel)]="resetToken" placeholder="Reset token" />
                                    </label>
                                    <label class="auth-login-input-wrap" *ngIf="resetToken">
                                        <i class="pi pi-lock"></i>
                                        <input class="auth-login-input" type="password" name="newPassword" [(ngModel)]="newPassword" placeholder="New password" />
                                    </label>
                                </div>
                                <button class="auth-login-submit" type="submit" [disabled]="auth.isLoading()">
                                    {{ resetToken ? 'Confirm Reset' : 'Send Reset Link' }}
                                </button>
                            </form>
                        </ng-container>

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
    readonly mode = signal<'login' | 'reset'>('login');

    loginEmail = '';
    loginPassword = '';
    resetEmail = '';
    resetToken = '';
    newPassword = '';

    login(): void {
        this.auth.login({ email: this.loginEmail.trim(), password: this.loginPassword.trim() }).subscribe({
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    requestReset(): void {
        if (this.resetToken && this.newPassword) {
            this.auth.confirmPasswordReset(this.resetEmail.trim(), this.resetToken.trim(), this.newPassword).subscribe({
                next: () => {
                    this.status.set('Password updated. You can sign in now.');
                    this.mode.set('login');
                },
                error: (err) => this.status.set(this.describeError(err))
            });
            return;
        }

        this.auth.requestPasswordReset(this.resetEmail.trim()).subscribe({
            next: (response) => {
                this.status.set(response.message);
                if (response.resetToken) {
                    this.resetToken = response.resetToken;
                }
            },
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err?.error ?? err, 'Request failed.');
    }
}
