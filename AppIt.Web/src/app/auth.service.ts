import { Injectable, signal } from '@angular/core';

export type UserRole = 'regular' | 'super';

export interface UserProfile {
  id: string;
  role: UserRole;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  avatarUrl?: string;
  preferredCurrency?: 'USD' | 'ZAR' | 'GBP';
}

interface StoredUser extends UserProfile {
  password: string;
}

const AUTH_KEY = 'appit.auth.user';
const USERS_KEY = 'appit.auth.users';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly user = signal<UserProfile | null>(null);

  constructor() {
    this.seedSuperUser();
    this.restoreUser();
  }

  login(email: string, password: string): { ok: boolean; message?: string } {
    const users = this.getUsers();
    const match = users.find((item) => item.email.toLowerCase() === email.toLowerCase());
    if (!match || match.password !== password) {
      return { ok: false, message: 'Invalid email or password.' };
    }

    this.setUser(match);
    return { ok: true };
  }

  register(profile: Omit<UserProfile, 'id' | 'role'> & { password: string }): { ok: boolean; message?: string } {
    const users = this.getUsers();
    const exists = users.some((item) => item.email.toLowerCase() === profile.email.toLowerCase());
    if (exists) {
      return { ok: false, message: 'Email already registered.' };
    }

    const next: StoredUser = {
      id: this.buildId(),
      role: 'regular',
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
      phone: profile.phone,
      avatarUrl: profile.avatarUrl,
      preferredCurrency: profile.preferredCurrency ?? 'USD',
      password: profile.password
    };

    users.push(next);
    this.saveUsers(users);
    this.setUser(next);
    return { ok: true };
  }

  createSuperUser(profile: Omit<UserProfile, 'id' | 'role'> & { password: string }): { ok: boolean; message?: string } {
    const users = this.getUsers();
    const exists = users.some((item) => item.email.toLowerCase() === profile.email.toLowerCase());
    if (exists) {
      return { ok: false, message: 'Email already registered.' };
    }

    const next: StoredUser = {
      id: this.buildId(),
      role: 'super',
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
      phone: profile.phone,
      avatarUrl: profile.avatarUrl,
      preferredCurrency: profile.preferredCurrency ?? 'USD',
      password: profile.password
    };

    users.push(next);
    this.saveUsers(users);
    return { ok: true };
  }

  updateProfile(patch: Partial<UserProfile>): void {
    const current = this.user();
    if (!current) {
      return;
    }

    const users = this.getUsers();
    const idx = users.findIndex((item) => item.id === current.id);
    if (idx >= 0) {
      users[idx] = { ...users[idx], ...patch };
      this.saveUsers(users);
    }

    this.setUser({ ...current, ...patch });
  }

  logout(): void {
    this.user.set(null);
    localStorage.removeItem(AUTH_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.user();
  }

  isSuperUser(): boolean {
    return this.user()?.role === 'super';
  }

  listUsers(): UserProfile[] {
    return this.getUsers().map(({ password, ...rest }) => rest);
  }

  private setUser(user: StoredUser | UserProfile): void {
    const { password, ...profile } = user as StoredUser;
    const payload = profile as UserProfile;
    this.user.set(payload);
    localStorage.setItem(AUTH_KEY, JSON.stringify(payload));
  }

  private restoreUser(): void {
    const raw = localStorage.getItem(AUTH_KEY);
    if (!raw) {
      return;
    }
    try {
      const parsed = JSON.parse(raw) as UserProfile;
      this.user.set(parsed);
    } catch {
      localStorage.removeItem(AUTH_KEY);
    }
  }

  private getUsers(): StoredUser[] {
    const raw = localStorage.getItem(USERS_KEY);
    if (!raw) {
      return [];
    }
    try {
      return JSON.parse(raw) as StoredUser[];
    } catch {
      return [];
    }
  }

  private saveUsers(users: StoredUser[]): void {
    localStorage.setItem(USERS_KEY, JSON.stringify(users));
  }

  private seedSuperUser(): void {
    const users = this.getUsers();
    if (users.length > 0) {
      return;
    }

    const seed: StoredUser = {
      id: this.buildId(),
      role: 'super',
      firstName: 'System',
      lastName: 'Administrator',
      email: 'admin@appit.com',
      phone: '+263 77 000 0000',
      avatarUrl: '',
      preferredCurrency: 'USD',
      password: 'Admin@2026'
    };

    this.saveUsers([seed]);
  }

  private buildId(): string {
    return `USR-${Date.now().toString(36).toUpperCase()}-${Math.floor(Math.random() * 9999)}`;
  }
}
