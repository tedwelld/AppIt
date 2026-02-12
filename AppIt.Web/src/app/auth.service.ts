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
  preferredLanguage?: 'en' | 'fr' | 'es';
  timezone?: string;
  emailNotifications?: boolean;
  smsNotifications?: boolean;
  marketingEmails?: boolean;
}

interface StoredUser extends UserProfile {
  password: string;
  isApproved?: boolean;
}

const AUTH_KEY = 'appit.auth.user';
const USERS_KEY = 'appit.auth.users';
const ONLINE_KEY = 'appit.auth.online';
const WELCOME_KEY = 'appit.auth.welcome';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly user = signal<UserProfile | null>(null);
  readonly welcomeMessage = signal('');

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
    if (match.isApproved === false) {
      return { ok: false, message: 'Account is waiting for super-user approval.' };
    }

    this.setUser(match);
    this.setWelcomeMessage(`Welcome ${match.firstName} ${match.lastName}`.trim());
    this.markOnline(match.email);
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
      password: profile.password,
      isApproved: false
    };

    users.push(next);
    this.saveUsers(users);
    return { ok: true, message: 'Account created and pending approval.' };
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
      password: profile.password,
      isApproved: true
    };

    users.push(next);
    this.saveUsers(users);
    return { ok: true };
  }

  updateProfile(patch: Partial<UserProfile>): void {
    this.updateAccountSettings(patch);
  }

  updateAccountSettings(patch: Partial<UserProfile>): { ok: boolean; message?: string } {
    const current = this.user();
    if (!current) {
      return { ok: false, message: 'No active user session.' };
    }

    const normalizedEmail = (patch.email ?? current.email).trim();
    const users = this.getUsers();
    const duplicateEmail = users.some(
      (item) => item.id !== current.id && item.email.toLowerCase() === normalizedEmail.toLowerCase()
    );
    if (duplicateEmail) {
      return { ok: false, message: 'Email is already in use by another account.' };
    }

    const idx = users.findIndex((item) => item.id === current.id);
    if (idx >= 0) {
      users[idx] = { ...users[idx], ...patch, email: normalizedEmail };
      this.saveUsers(users);
    }

    this.setUser({ ...current, ...patch, email: normalizedEmail });
    if (current.email.toLowerCase() !== normalizedEmail.toLowerCase()) {
      this.markOffline(current.email);
      this.markOnline(normalizedEmail);
    }

    return { ok: true };
  }

  changePassword(currentPassword: string, newPassword: string): { ok: boolean; message?: string } {
    const current = this.user();
    if (!current) {
      return { ok: false, message: 'No active user session.' };
    }

    const users = this.getUsers();
    const idx = users.findIndex((item) => item.id === current.id);
    if (idx < 0) {
      return { ok: false, message: 'User account not found.' };
    }

    if (users[idx].password !== currentPassword) {
      return { ok: false, message: 'Current password is incorrect.' };
    }

    if (newPassword.trim().length < 6) {
      return { ok: false, message: 'New password must be at least 6 characters.' };
    }

    users[idx].password = newPassword.trim();
    this.saveUsers(users);
    return { ok: true, message: 'Password updated.' };
  }

  logout(): void {
    const current = this.user();
    if (current?.email) {
      this.markOffline(current.email);
    }
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

  listOnlineUsers(): UserProfile[] {
    const users = this.getUsers();
    const onlineMap = this.getOnlineMap();
    return users
      .filter((u) => onlineMap[u.email.toLowerCase()])
      .map(({ password, ...rest }) => rest);
  }

  listPendingUsers(): UserProfile[] {
    return this.getUsers()
      .filter((u) => u.isApproved === false)
      .map(({ password, ...rest }) => rest);
  }

  consumeWelcomeMessage(): string {
    const message = this.welcomeMessage();
    this.welcomeMessage.set('');
    localStorage.removeItem(WELCOME_KEY);
    return message;
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
      this.markOnline(parsed.email);
      const welcome = localStorage.getItem(WELCOME_KEY);
      if (welcome) {
        this.welcomeMessage.set(welcome);
      }
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

  private setWelcomeMessage(message: string): void {
    this.welcomeMessage.set(message);
    localStorage.setItem(WELCOME_KEY, message);
  }

  private getOnlineMap(): Record<string, string> {
    const raw = localStorage.getItem(ONLINE_KEY);
    if (!raw) {
      return {};
    }
    try {
      return JSON.parse(raw) as Record<string, string>;
    } catch {
      return {};
    }
  }

  private saveOnlineMap(map: Record<string, string>): void {
    localStorage.setItem(ONLINE_KEY, JSON.stringify(map));
  }

  private markOnline(email: string): void {
    if (!email) {
      return;
    }
    const map = this.getOnlineMap();
    map[email.toLowerCase()] = new Date().toISOString();
    this.saveOnlineMap(map);
  }

  private markOffline(email: string): void {
    if (!email) {
      return;
    }
    const map = this.getOnlineMap();
    delete map[email.toLowerCase()];
    this.saveOnlineMap(map);
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
      password: 'Admin@2026',
      isApproved: true
    };

    this.saveUsers([seed]);
  }

  private buildId(): string {
    return `USR-${Date.now().toString(36).toUpperCase()}-${Math.floor(Math.random() * 9999)}`;
  }
}
