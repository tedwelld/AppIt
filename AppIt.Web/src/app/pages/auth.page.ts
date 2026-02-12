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
      <div class="auth-panel">
        <header>
          <p class="eyebrow">Welcome to AppIt</p>
          <h1>Sign in or create your guest account</h1>
          <p class="sub">Super users are created by administrators only.</p>
        </header>

        <div class="tab-row">
          <button class="tab" [class.active]="tab() === 'login'" (click)="tab.set('login')">Sign In</button>
          <button class="tab" [class.active]="tab() === 'register'" (click)="tab.set('register')">Register</button>
        </div>

        <form *ngIf="tab() === 'login'" (ngSubmit)="submitLogin()" class="form">
          <div class="field">
            <label>Email</label>
            <input class="app-input" type="email" [(ngModel)]="loginEmail" name="loginEmail" required />
          </div>
          <div class="field">
            <label>Password</label>
            <input class="app-input" type="password" [(ngModel)]="loginPassword" name="loginPassword" required />
          </div>
          <button class="btn-base btn-primary" type="submit">Sign In</button>
          <p class="status" *ngIf="status()">{{ status() }}</p>
          <p class="hint">Default admin: admin@appit.com / Admin@2026</p>
        </form>

        <form *ngIf="tab() === 'register'" (ngSubmit)="submitRegister()" class="form">
          <div class="field-grid">
            <div class="field">
              <label>First Name</label>
              <input class="app-input" type="text" [(ngModel)]="register.firstName" name="firstName" required />
            </div>
            <div class="field">
              <label>Last Name</label>
              <input class="app-input" type="text" [(ngModel)]="register.lastName" name="lastName" required />
            </div>
          </div>
          <div class="field">
            <label>Email</label>
            <input class="app-input" type="email" [(ngModel)]="register.email" name="registerEmail" required />
          </div>
          <div class="field">
            <label>Phone</label>
            <input class="app-input" type="text" [(ngModel)]="register.phone" name="registerPhone" />
          </div>
          <div class="field-grid">
            <div class="field">
              <label>Password</label>
              <input class="app-input" type="password" [(ngModel)]="register.password" name="registerPassword" required />
            </div>
            <div class="field">
              <label>Preferred Currency</label>
              <select class="app-select" [(ngModel)]="register.preferredCurrency" name="registerCurrency">
                <option value="USD">USD</option>
                <option value="ZAR">ZAR</option>
                <option value="GBP">GBP</option>
              </select>
            </div>
          </div>
          <button class="btn-base btn-primary" type="submit">Create Account</button>
          <p class="status" *ngIf="status()">{{ status() }}</p>
        </form>
      </div>

      <aside class="promo">
        <div class="promo-card">
          <h2>Adventure, Accommodation, Activities</h2>
          <p>
            Manage reservations, create invoices, and keep payments in sync across USD, ZAR, and GBP.
            Your vouchers stay linked, even for multi-service combos.
          </p>
          <ul>
            <li>Single, Double, Express, and Standard rooms</li>
            <li>Bungee, Swing, Bridge Tours, Falls View, Boat Cruise</li>
            <li>Instant vouchers and invoice statuses</li>
          </ul>
        </div>
      </aside>
    </section>
  `,
  styles: `
    .auth {
      display: grid;
      grid-template-columns: minmax(280px, 420px) 1fr;
      gap: 1.2rem;
      min-height: 100%;
    }
    .auth-panel {
      border: 1px solid #dbe3ef;
      border-radius: 1.2rem;
      padding: 1rem;
      background: linear-gradient(160deg, #ffffff, #f6fbff);
      box-shadow: 0 20px 45px rgba(15, 48, 84, 0.08);
      display: grid;
      gap: 0.8rem;
      align-content: start;
    }
    header h1 { margin: 0.2rem 0; font-size: 1.4rem; }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.12em; font-size: 0.72rem; color: #0f4c5c; font-weight: 700; margin: 0; }
    .sub { margin: 0; color: #5b6f85; font-size: 0.85rem; }
    .tab-row { display: flex; gap: 0.4rem; }
    .tab {
      flex: 1;
      border: 1px solid #c7d3e0;
      border-radius: 999px;
      padding: 0.4rem 0.6rem;
      background: #fff;
      font-weight: 700;
      cursor: pointer;
    }
    .tab.active {
      background: #0f4c5c;
      color: #fff;
      border-color: #0f4c5c;
    }
    .form { display: grid; gap: 0.6rem; }
    .field { display: grid; gap: 0.3rem; }
    .field-grid { display: grid; gap: 0.6rem; grid-template-columns: repeat(2, minmax(0, 1fr)); }
    label { font-size: 0.8rem; font-weight: 700; color: #43576d; }
    .status { margin: 0; color: #0f4c5c; background: #eaf7f5; border-radius: 0.7rem; padding: 0.4rem 0.6rem; border: 1px solid #cce8e3; }
    .hint { margin: 0; font-size: 0.72rem; color: #74849a; }
    .promo {
      border-radius: 1.2rem;
      background: linear-gradient(135deg, #0f4c5c, #2f8c88 60%, #82c0cc);
      color: #fff;
      padding: 1.2rem;
      display: grid;
      align-items: center;
    }
    .promo-card { display: grid; gap: 0.7rem; max-width: 480px; }
    .promo-card h2 { margin: 0; font-size: 1.6rem; }
    .promo-card p { margin: 0; line-height: 1.5; }
    .promo-card ul { margin: 0; padding-left: 1rem; }
    .promo-card li { margin-bottom: 0.4rem; }
    @media (max-width: 980px) {
      .auth { grid-template-columns: 1fr; }
      .promo { order: -1; }
      .field-grid { grid-template-columns: 1fr; }
    }
  `
})
export class AuthPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly tab = signal<'login' | 'register'>('login');
  readonly status = signal('');

  loginEmail = '';
  loginPassword = '';

  register = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    password: '',
    preferredCurrency: 'USD'
  };

  submitLogin(): void {
    const result = this.auth.login(this.loginEmail.trim(), this.loginPassword.trim());
    if (!result.ok) {
      this.status.set(result.message ?? 'Login failed.');
      return;
    }

    const target = this.auth.isSuperUser() ? '/admin' : '/user';
    this.router.navigateByUrl(target);
  }

  submitRegister(): void {
    const result = this.auth.register({
      firstName: this.register.firstName.trim(),
      lastName: this.register.lastName.trim(),
      email: this.register.email.trim(),
      phone: this.register.phone.trim(),
      password: this.register.password.trim(),
      preferredCurrency: this.register.preferredCurrency as 'USD' | 'ZAR' | 'GBP'
    });

    if (!result.ok) {
      this.status.set(result.message ?? 'Registration failed.');
      return;
    }

    this.router.navigateByUrl('/user');
  }
}
