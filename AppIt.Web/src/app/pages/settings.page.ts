import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="settings">
      <header class="hero">
        <div>
          <p class="eyebrow">Settings</p>
          <h1>Profile & Preferences</h1>
          <p class="sub">Manage your personal data and default currency.</p>
        </div>
      </header>

      <div class="grid">
        <article class="panel">
          <h2>My Profile</h2>
          <form (ngSubmit)="saveProfile()" class="form">
            <label>First Name</label>
            <input class="app-input" [(ngModel)]="profile.firstName" name="firstName" />

            <label>Last Name</label>
            <input class="app-input" [(ngModel)]="profile.lastName" name="lastName" />

            <label>Email</label>
            <input class="app-input" type="email" [(ngModel)]="profile.email" name="email" />

            <label>Phone</label>
            <input class="app-input" [(ngModel)]="profile.phone" name="phone" />

            <label>Avatar URL</label>
            <input class="app-input" [(ngModel)]="profile.avatarUrl" name="avatarUrl" />

            <label>Preferred Currency</label>
            <select class="app-select" [(ngModel)]="profile.preferredCurrency" name="currency">
              <option value="USD">USD</option>
              <option value="ZAR">ZAR</option>
              <option value="GBP">GBP</option>
            </select>

            <button class="btn-base btn-primary" type="submit">Save Changes</button>
            <p class="status" *ngIf="status()">{{ status() }}</p>
          </form>
        </article>

        <article class="panel" *ngIf="isSuperUser()">
          <h2>Create Super User</h2>
          <form (ngSubmit)="createSuperUser()" class="form">
            <label>First Name</label>
            <input class="app-input" [(ngModel)]="superUser.firstName" name="suFirstName" />

            <label>Last Name</label>
            <input class="app-input" [(ngModel)]="superUser.lastName" name="suLastName" />

            <label>Email</label>
            <input class="app-input" type="email" [(ngModel)]="superUser.email" name="suEmail" />

            <label>Phone</label>
            <input class="app-input" [(ngModel)]="superUser.phone" name="suPhone" />

            <label>Password</label>
            <input class="app-input" type="password" [(ngModel)]="superUser.password" name="suPassword" />

            <button class="btn-base btn-primary" type="submit">Create Super User</button>
            <p class="status" *ngIf="superStatus()">{{ superStatus() }}</p>
          </form>
        </article>
      </div>
    </section>
  `,
  styles: `
    .settings { display: grid; gap: 0.9rem; }
    .hero {
      border: 1px solid #dbe3ef;
      border-radius: 1rem;
      padding: 0.8rem;
      background: linear-gradient(150deg, #ffffff, #f4f9ff);
    }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.12em; font-size: 0.7rem; color: #0f4c5c; font-weight: 700; }
    .sub { margin: 0; color: #5b6f85; font-size: 0.85rem; }
    .grid { display: grid; gap: 0.8rem; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); }
    .panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      padding: 0.8rem;
      background: #fff;
      display: grid;
      gap: 0.6rem;
    }
    .panel h2 { margin: 0; font-size: 1.05rem; color: #14334f; }
    .form { display: grid; gap: 0.45rem; }
    .status { margin: 0; font-size: 0.8rem; color: #0f4c5c; background: #eaf7f5; border: 1px solid #cce8e3; border-radius: 0.7rem; padding: 0.4rem 0.6rem; }
  `
})
export class SettingsPageComponent {
  private readonly auth = inject(AuthService);

  readonly status = signal('');
  readonly superStatus = signal('');
  readonly isSuperUser = signal(this.auth.isSuperUser());

  profile = {
    firstName: this.auth.user()?.firstName ?? '',
    lastName: this.auth.user()?.lastName ?? '',
    email: this.auth.user()?.email ?? '',
    phone: this.auth.user()?.phone ?? '',
    avatarUrl: this.auth.user()?.avatarUrl ?? '',
    preferredCurrency: this.auth.user()?.preferredCurrency ?? 'USD'
  };

  superUser = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    password: ''
  };

  saveProfile(): void {
    this.auth.updateProfile({
      firstName: this.profile.firstName.trim(),
      lastName: this.profile.lastName.trim(),
      email: this.profile.email.trim(),
      phone: this.profile.phone.trim(),
      avatarUrl: this.profile.avatarUrl.trim(),
      preferredCurrency: this.profile.preferredCurrency as 'USD' | 'ZAR' | 'GBP'
    });

    this.status.set('Profile saved.');
  }

  createSuperUser(): void {
    const result = this.auth.createSuperUser({
      firstName: this.superUser.firstName.trim(),
      lastName: this.superUser.lastName.trim(),
      email: this.superUser.email.trim(),
      phone: this.superUser.phone.trim(),
      password: this.superUser.password.trim(),
      preferredCurrency: 'USD'
    });

    if (!result.ok) {
      this.superStatus.set(result.message ?? 'Failed to create user.');
      return;
    }

    this.superStatus.set('Super user created.');
    this.superUser = { firstName: '', lastName: '', email: '', phone: '', password: '' };
  }
}
