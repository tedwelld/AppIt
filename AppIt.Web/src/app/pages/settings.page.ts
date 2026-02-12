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
          <h1>Profile, Preferences, and Security</h1>
          <p class="sub">Manage account details you are allowed to change.</p>
        </div>
      </header>

      <div class="grid">
        <article class="panel">
          <h2>Profile Photo</h2>
          <div class="avatar-wrap">
            <div class="avatar">
              <img *ngIf="profile.avatarUrl" [src]="profile.avatarUrl" alt="Profile avatar" />
              <span *ngIf="!profile.avatarUrl">{{ initials() }}</span>
            </div>
            <div class="avatar-actions">
              <input type="file" accept="image/*" (change)="onAvatarSelected($event)" />
              <button class="btn-base btn-secondary" type="button" (click)="removeAvatar()">Remove Photo</button>
            </div>
          </div>
        </article>

        <article class="panel">
          <h2>Personal Details</h2>
          <form (ngSubmit)="saveProfile()" class="form">
            <label>First Name</label>
            <input class="app-input" [(ngModel)]="profile.firstName" name="firstName" />

            <label>Last Name</label>
            <input class="app-input" [(ngModel)]="profile.lastName" name="lastName" />

            <label>Email</label>
            <input class="app-input" type="email" [(ngModel)]="profile.email" name="email" />

            <label>Phone Number</label>
            <input class="app-input" [(ngModel)]="profile.phone" name="phone" />

            <label>Preferred Currency</label>
            <select class="app-select" [(ngModel)]="profile.preferredCurrency" name="currency">
              <option value="USD">USD</option>
              <option value="ZAR">ZAR</option>
              <option value="GBP">GBP</option>
            </select>

            <label>Language</label>
            <select class="app-select" [(ngModel)]="profile.preferredLanguage" name="language">
              <option value="en">English</option>
              <option value="fr">French</option>
              <option value="es">Spanish</option>
            </select>

            <label>Timezone</label>
            <input class="app-input" [(ngModel)]="profile.timezone" name="timezone" />

            <button class="btn-base btn-primary" type="submit">Save Profile Settings</button>
            <p class="status" *ngIf="status()">{{ status() }}</p>
          </form>
        </article>

        <article class="panel">
          <h2>Notifications</h2>
          <form (ngSubmit)="saveProfile()" class="form">
            <label class="check">
              <input type="checkbox" [(ngModel)]="profile.emailNotifications" name="emailNotifications" />
              Email Notifications
            </label>
            <label class="check">
              <input type="checkbox" [(ngModel)]="profile.smsNotifications" name="smsNotifications" />
              SMS Notifications
            </label>
            <label class="check">
              <input type="checkbox" [(ngModel)]="profile.marketingEmails" name="marketingEmails" />
              Marketing Emails
            </label>
            <button class="btn-base btn-primary" type="submit">Save Notification Settings</button>
          </form>
        </article>

        <article class="panel">
          <h2>Security</h2>
          <form (ngSubmit)="changePassword()" class="form">
            <label>Current Password</label>
            <input class="app-input" type="password" [(ngModel)]="password.current" name="currentPassword" />

            <label>New Password</label>
            <input class="app-input" type="password" [(ngModel)]="password.next" name="newPassword" />

            <label>Confirm New Password</label>
            <input class="app-input" type="password" [(ngModel)]="password.confirm" name="confirmPassword" />

            <button class="btn-base btn-primary" type="submit">Update Password</button>
            <p class="status" *ngIf="passwordStatus()">{{ passwordStatus() }}</p>
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
    .settings { display: grid; gap: 0.7rem; height: 100%; min-height: 0; overflow: auto; align-content: start; }
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
      align-content: start;
      max-height: 430px;
      overflow: auto;
    }
    .panel h2 { margin: 0; font-size: 1.05rem; color: #14334f; }
    .form { display: grid; gap: 0.45rem; }
    .avatar-wrap { display: grid; gap: 0.6rem; }
    .avatar {
      width: 5rem;
      height: 5rem;
      border-radius: 1rem;
      background: linear-gradient(145deg, #0f4c5c, #2f8c88);
      color: #fff;
      display: grid;
      place-items: center;
      font-size: 1.1rem;
      font-weight: 800;
      overflow: hidden;
    }
    .avatar img { width: 100%; height: 100%; object-fit: cover; }
    .avatar-actions { display: grid; gap: 0.4rem; }
    .check { display: inline-flex; align-items: center; gap: 0.45rem; font-size: 0.88rem; }
    .status { margin: 0; font-size: 0.8rem; color: #0f4c5c; background: #eaf7f5; border: 1px solid #cce8e3; border-radius: 0.7rem; padding: 0.4rem 0.6rem; }
  `
})
export class SettingsPageComponent {
  private readonly auth = inject(AuthService);

  readonly status = signal('');
  readonly superStatus = signal('');
  readonly passwordStatus = signal('');
  readonly isSuperUser = signal(this.auth.isSuperUser());

  profile = {
    firstName: this.auth.user()?.firstName ?? '',
    lastName: this.auth.user()?.lastName ?? '',
    email: this.auth.user()?.email ?? '',
    phone: this.auth.user()?.phone ?? '',
    avatarUrl: this.auth.user()?.avatarUrl ?? '',
    preferredCurrency: this.auth.user()?.preferredCurrency ?? 'USD',
    preferredLanguage: this.auth.user()?.preferredLanguage ?? 'en',
    timezone: this.auth.user()?.timezone ?? Intl.DateTimeFormat().resolvedOptions().timeZone,
    emailNotifications: this.auth.user()?.emailNotifications ?? true,
    smsNotifications: this.auth.user()?.smsNotifications ?? false,
    marketingEmails: this.auth.user()?.marketingEmails ?? false
  };

  password = {
    current: '',
    next: '',
    confirm: ''
  };

  superUser = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    password: ''
  };

  initials(): string {
    const first = this.profile.firstName?.[0] ?? '';
    const last = this.profile.lastName?.[0] ?? '';
    return `${first}${last}`.toUpperCase() || 'U';
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0];
    if (!file) {
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.profile.avatarUrl = String(reader.result ?? '');
    };
    reader.readAsDataURL(file);
  }

  removeAvatar(): void {
    this.profile.avatarUrl = '';
  }

  saveProfile(): void {
    const result = this.auth.updateAccountSettings({
      firstName: this.profile.firstName.trim(),
      lastName: this.profile.lastName.trim(),
      email: this.profile.email.trim(),
      phone: this.profile.phone.trim(),
      avatarUrl: this.profile.avatarUrl.trim(),
      preferredCurrency: this.profile.preferredCurrency as 'USD' | 'ZAR' | 'GBP',
      preferredLanguage: this.profile.preferredLanguage as 'en' | 'fr' | 'es',
      timezone: this.profile.timezone.trim(),
      emailNotifications: !!this.profile.emailNotifications,
      smsNotifications: !!this.profile.smsNotifications,
      marketingEmails: !!this.profile.marketingEmails
    });

    this.status.set(result.ok ? 'Settings saved.' : (result.message ?? 'Failed to save settings.'));
  }

  changePassword(): void {
    if (this.password.next.trim() !== this.password.confirm.trim()) {
      this.passwordStatus.set('New password and confirmation do not match.');
      return;
    }

    const result = this.auth.changePassword(this.password.current, this.password.next);
    this.passwordStatus.set(result.message ?? (result.ok ? 'Password updated.' : 'Failed to update password.'));
    if (result.ok) {
      this.password = { current: '', next: '', confirm: '' };
    }
  }

  createSuperUser(): void {
    const result = this.auth.createSuperUser({
      firstName: this.superUser.firstName.trim(),
      lastName: this.superUser.lastName.trim(),
      email: this.superUser.email.trim(),
      phone: this.superUser.phone.trim(),
      password: this.superUser.password.trim(),
      preferredCurrency: 'USD',
      preferredLanguage: 'en',
      timezone: 'UTC',
      emailNotifications: true,
      smsNotifications: false,
      marketingEmails: false
    });

    if (!result.ok) {
      this.superStatus.set(result.message ?? 'Failed to create user.');
      return;
    }

    this.superStatus.set('Super user created.');
    this.superUser = { firstName: '', lastName: '', email: '', phone: '', password: '' };
  }
}
