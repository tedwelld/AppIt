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
    <section class="auth row justify-content-center">
      <article class="auth-panel card o-hidden border-0 shadow-lg">
        <div class="card-body p-0">
          <div class="row no-gutters">
            <div class="col-lg-5 d-none d-lg-block auth-art"></div>
            <div class="col-lg-7">
              <div class="p-5">
                <div class="text-center">
                  <p class="text-xs font-weight-bold text-primary text-uppercase mb-1">AppIt Secure Access</p>
                  <h1 class="h4 text-gray-900 mb-2">{{ mode() === 'login' ? 'Welcome Back' : mode() === 'signup' ? 'Create Account' : 'Reset Password' }}</h1>
                  <p class="sub" *ngIf="mode() === 'login'">Sign in to continue.</p>
                  <p class="sub" *ngIf="mode() === 'signup'">Create a simple user account to start booking and reservations.</p>
                  <p class="sub" *ngIf="mode() === 'reset'">Set a new password for existing accounts.</p>
                </div>

                <div class="mode-switch btn-group w-100 my-4">
                  <button class="btn btn-outline-primary" type="button" [class.active]="mode() === 'login'" (click)="switchMode('login')">Login</button>
                  <button class="btn btn-outline-primary" type="button" [class.active]="mode() === 'signup'" (click)="switchMode('signup')">Sign Up</button>
                  <button class="btn btn-outline-primary" type="button" [class.active]="mode() === 'reset'" (click)="switchMode('reset')">Reset</button>
                </div>

        <form *ngIf="mode() === 'login'" (ngSubmit)="submitLogin()" class="form user">
          <label>Email</label>
          <input class="form-control form-control-user" type="email" [(ngModel)]="loginEmail" name="loginEmail" required />

          <label>Password</label>
          <input class="form-control form-control-user" type="password" [(ngModel)]="loginPassword" name="loginPassword" required />

          <button class="btn btn-primary btn-user btn-block" type="submit">Sign In</button>
        </form>

        <form *ngIf="mode() === 'signup'" (ngSubmit)="submitSignup()" class="form user">
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

          <button class="btn btn-primary btn-user btn-block" type="submit">Create Account</button>
        </form>

        <form *ngIf="mode() === 'reset'" (ngSubmit)="confirmReset()" class="form user">
          <label>Email</label>
          <input class="app-input" type="email" [(ngModel)]="reset.email" name="resetEmail" required />

          <button class="btn btn-outline-primary btn-user btn-block" type="button" (click)="requestResetToken()">Request Reset Token</button>

          <label>Reset Token</label>
          <input class="app-input" type="text" [(ngModel)]="reset.token" name="resetToken" required />

          <label>New Password</label>
          <input class="app-input" type="password" [(ngModel)]="reset.newPassword" name="resetNewPassword" minlength="8" required />

          <label>Confirm New Password</label>
          <input class="app-input" type="password" [(ngModel)]="reset.confirmPassword" name="resetConfirmPassword" minlength="8" required />

          <button class="btn btn-primary btn-user btn-block" type="submit">Set New Password</button>
          <p class="hint" *ngIf="reset.devToken">Dev reset token: {{ reset.devToken }}</p>
        </form>

                <p class="status alert alert-info mt-3 mb-0" *ngIf="status()">{{ status() }}</p>
              </div>
            </div>
          </div>
        </div>
      </article>
    </section>
  `,
  styles: `
    .auth { min-height: 100%; padding: 1rem; align-items: center; }
    .auth-panel {
      width: min(960px, 100%);
    }
    .auth-art {
      background: linear-gradient(rgba(78, 115, 223, 0.5), rgba(34, 74, 190, 0.62)), url('/template/img/undraw_posting_photo.svg') center / min(72%, 420px) no-repeat;
      min-height: 520px;
    }
    .sub { margin: 0; color: #5b6f85; font-size: 0.9rem; }
    .form { display: grid; gap: 0.55rem; margin-top: 0.2rem; }
    label { font-size: 0.82rem; font-weight: 700; color: #43576d; }
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
