import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="auth">
      <article class="auth-panel glass-card">
        <p class="eyebrow">AppIt Secure Access</p>
        <h1>{{ mode() === 'login' ? 'Sign In' : mode() === 'signup' ? 'Create Account' : 'Reset Password' }}</h1>
        <p class="sub" *ngIf="mode() === 'login'">Sign in to continue.</p>
        <p class="sub" *ngIf="mode() === 'signup'">Create a simple user account to start booking and reservations.</p>
        <p class="sub" *ngIf="mode() === 'reset'">Set a new password for existing accounts (including legacy accounts).</p>

        <div class="mode-switch">
          <button class="btn-base btn-secondary" type="button" [class.active]="mode() === 'login'" (click)="switchMode('login')">Login</button>
          <button class="btn-base btn-secondary" type="button" [class.active]="mode() === 'signup'" (click)="switchMode('signup')">Sign Up</button>
          <button class="btn-base btn-secondary" type="button" [class.active]="mode() === 'reset'" (click)="switchMode('reset')">Reset</button>
        </div>

        <form *ngIf="mode() === 'login'" (ngSubmit)="submitLogin()" class="form">
          <label>Email</label>
          <input class="app-input" type="email" [(ngModel)]="loginEmail" name="loginEmail" required />

          <label>Password</label>
          <input class="app-input" type="password" [(ngModel)]="loginPassword" name="loginPassword" required />

          <button class="btn-base btn-primary" type="submit">Sign In</button>
          <p class="hint">Default admin: admin@appit.com / Admin@2026</p>
        </form>

        <form *ngIf="mode() === 'signup'" (ngSubmit)="submitSignup()" class="form">
          <label>First Name</label>
          <input class="app-input" type="text" [(ngModel)]="signup.firstName" name="signupFirstName" required />

          <label>Last Name</label>
          <input class="app-input" type="text" [(ngModel)]="signup.lastName" name="signupLastName" required />

          <label>Email</label>
          <input class="app-input" type="email" [(ngModel)]="signup.email" name="signupEmail" required />

          <label>Phone</label>
          <input class="app-input" type="text" [(ngModel)]="signup.phone" name="signupPhone" />

          <label>Preferred Currency</label>
          <select class="app-select" [(ngModel)]="signup.preferredCurrency" name="signupCurrency">
            <option value="USD">USD</option>
            <option value="ZAR">ZAR</option>
            <option value="GBP">GBP</option>
          </select>

          <label>Password</label>
          <input class="app-input" type="password" [(ngModel)]="signup.password" name="signupPassword" minlength="8" required />

          <label>Confirm Password</label>
          <input class="app-input" type="password" [(ngModel)]="signup.confirmPassword" name="signupConfirmPassword" minlength="8" required />

          <button class="btn-base btn-primary" type="submit">Create Account</button>
        </form>

        <form *ngIf="mode() === 'reset'" (ngSubmit)="confirmReset()" class="form">
          <label>Email</label>
          <input class="app-input" type="email" [(ngModel)]="reset.email" name="resetEmail" required />

          <button class="btn-base btn-secondary" type="button" (click)="requestResetToken()">Request Reset Token</button>

          <label>Reset Token</label>
          <input class="app-input" type="text" [(ngModel)]="reset.token" name="resetToken" required />

          <label>New Password</label>
          <input class="app-input" type="password" [(ngModel)]="reset.newPassword" name="resetNewPassword" minlength="8" required />

          <label>Confirm New Password</label>
          <input class="app-input" type="password" [(ngModel)]="reset.confirmPassword" name="resetConfirmPassword" minlength="8" required />

          <button class="btn-base btn-primary" type="submit">Set New Password</button>
          <p class="hint" *ngIf="reset.devToken">Dev reset token: {{ reset.devToken }}</p>
        </form>

        <p class="status" *ngIf="status()">{{ status() }}</p>
      </article>
    </section>
  `,
  styles: `
    .auth {
      min-height: 100%;
      display: grid;
      place-items: center;
      padding: 1rem;
    }
    .auth-panel {
      width: min(560px, 100%);
      border-radius: 1.4rem;
      padding: 1.2rem;
      display: grid;
      gap: 0.7rem;
    }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.72rem; color: #0f4c5c; font-weight: 700; }
    h1 { margin: 0; font-size: 1.8rem; }
    .sub { margin: 0; color: #5b6f85; font-size: 0.9rem; }
    .mode-switch { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 0.45rem; }
    .mode-switch .active { border-color: #0f4c5c; color: #0f4c5c; }
    .form { display: grid; gap: 0.55rem; margin-top: 0.2rem; }
    label { font-size: 0.82rem; font-weight: 700; color: #43576d; }
    .status { margin: 0; color: #0f4c5c; background: #eaf7f5; border-radius: 0.7rem; padding: 0.4rem 0.6rem; border: 1px solid #cce8e3; }
    .hint { margin: 0.1rem 0 0; font-size: 0.72rem; color: #74849a; word-break: break-all; }
    @media (max-width: 640px) {
      .mode-switch { grid-template-columns: 1fr; }
    }
  `
})
export class AuthPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly status = signal('');
  readonly mode = signal<'login' | 'signup' | 'reset'>('login');

  loginEmail = '';
  loginPassword = '';

  signup = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    preferredCurrency: 'USD' as 'USD' | 'ZAR' | 'GBP',
    password: '',
    confirmPassword: ''
  };

  reset = {
    email: '',
    token: '',
    devToken: '',
    newPassword: '',
    confirmPassword: ''
  };

  switchMode(next: 'login' | 'signup' | 'reset'): void {
    this.mode.set(next);
    this.status.set('');
  }

  async submitLogin(): Promise<void> {
    const result = await this.auth.login(this.loginEmail.trim(), this.loginPassword.trim());
    if (!result.ok) {
      this.status.set(result.message ?? 'Login failed.');
      return;
    }

    this.router.navigateByUrl(this.auth.isSuperUser() ? '/admin' : '/user');
  }

  async submitSignup(): Promise<void> {
    if (this.signup.password.trim() !== this.signup.confirmPassword.trim()) {
      this.status.set('Password and confirmation do not match.');
      return;
    }

    if (this.signup.password.trim().length < 8) {
      this.status.set('Password must be at least 8 characters.');
      return;
    }

    const result = await this.auth.register({
      firstName: this.signup.firstName.trim(),
      lastName: this.signup.lastName.trim(),
      email: this.signup.email.trim(),
      phone: this.signup.phone.trim(),
      preferredCurrency: this.signup.preferredCurrency,
      password: this.signup.password.trim()
    });

    if (!result.ok) {
      this.status.set(result.message ?? 'Signup failed.');
      return;
    }

    this.status.set('Account created successfully. Redirecting...');
    this.router.navigateByUrl('/user');
  }

  async requestResetToken(): Promise<void> {
    if (!this.reset.email.trim()) {
      this.status.set('Enter your email address first.');
      return;
    }

    const result = await this.auth.requestPasswordReset(this.reset.email.trim());
    this.status.set(result.message);

    if (result.resetToken) {
      this.reset.token = result.resetToken;
      this.reset.devToken = result.resetToken;
    }
  }

  async confirmReset(): Promise<void> {
    if (this.reset.newPassword.trim() !== this.reset.confirmPassword.trim()) {
      this.status.set('New password and confirmation do not match.');
      return;
    }

    if (this.reset.newPassword.trim().length < 8) {
      this.status.set('New password must be at least 8 characters.');
      return;
    }

    const result = await this.auth.confirmPasswordReset(
      this.reset.email.trim(),
      this.reset.token.trim(),
      this.reset.newPassword.trim()
    );

    if (!result.ok) {
      this.status.set(result.message ?? 'Password reset failed.');
      return;
    }

    this.status.set(result.message ?? 'Password has been reset.');
    this.reset.token = '';
    this.reset.newPassword = '';
    this.reset.confirmPassword = '';
    this.mode.set('login');
  }
}
