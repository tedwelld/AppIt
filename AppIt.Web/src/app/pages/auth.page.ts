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
        <h1>Welcome Back</h1>
        <p class="sub">Sign in to continue.</p>

        <form (ngSubmit)="submitLogin()" class="form">
          <label>Email</label>
          <input class="app-input" type="email" [(ngModel)]="loginEmail" name="loginEmail" required />

          <label>Password</label>
          <input class="app-input" type="password" [(ngModel)]="loginPassword" name="loginPassword" required />

          <button class="btn-base btn-primary" type="submit">Sign In</button>
          <p class="status" *ngIf="status()">{{ status() }}</p>
          <p class="hint">Default admin: admin@appit.com / Admin@2026</p>
        </form>
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
      width: min(460px, 100%);
      border-radius: 1.4rem;
      padding: 1.2rem;
      display: grid;
      gap: 0.7rem;
    }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.72rem; color: #0f4c5c; font-weight: 700; }
    h1 { margin: 0; font-size: 1.8rem; }
    .sub { margin: 0; color: #5b6f85; font-size: 0.9rem; }
    .form { display: grid; gap: 0.55rem; margin-top: 0.3rem; }
    label { font-size: 0.82rem; font-weight: 700; color: #43576d; }
    .status { margin: 0; color: #0f4c5c; background: #eaf7f5; border-radius: 0.7rem; padding: 0.4rem 0.6rem; border: 1px solid #cce8e3; }
    .hint { margin: 0.1rem 0 0; font-size: 0.72rem; color: #74849a; }
  `
})
export class AuthPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly status = signal('');
  loginEmail = '';
  loginPassword = '';

  submitLogin(): void {
    const result = this.auth.login(this.loginEmail.trim(), this.loginPassword.trim());
    if (!result.ok) {
      this.status.set(result.message ?? 'Login failed.');
      return;
    }

    this.router.navigateByUrl(this.auth.isSuperUser() ? '/admin' : '/user');
  }
}
