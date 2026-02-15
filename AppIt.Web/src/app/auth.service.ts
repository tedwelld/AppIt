import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type UserRole = 'regular' | 'super' | 'admin';

export interface UserProfile {
  id: string;
  role: UserRole;
  roleId?: number;
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
  isActive?: boolean;
}

interface AuthResponse {
  user: {
    id: number;
    role: string;
    roleId?: number;
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    avatarUrl?: string;
    preferredCurrency?: string;
    isActive?: boolean;
  };
}

interface PasswordResetRequestResponse {
  message?: string;
  resetToken?: string;
}

const AUTH_KEY = 'appit.auth.user';
const ONLINE_KEY = 'appit.auth.online';
const WELCOME_KEY = 'appit.auth.welcome';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  readonly user = signal<UserProfile | null>(null);
  readonly welcomeMessage = signal('');
  readonly usersCache = signal<UserProfile[]>([]);

  constructor() {
    this.restoreSession();
  }

  async login(email: string, password: string): Promise<{ ok: boolean; message?: string }> {
    try {
      const payload = await firstValueFrom(this.http.post<unknown>('/api/auth/login', { email, password }));
      const response = this.unwrap(payload) as AuthResponse;
      if (!response?.user?.id) {
        return { ok: false, message: 'Invalid login response.' };
      }

      this.setUserProfile(this.mapUser(response.user));
      this.setWelcomeMessage(`Welcome ${response.user.firstName} ${response.user.lastName}`.trim());
      this.markOnline(response.user.email);
      this.refreshUsersCache();
      return { ok: true };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  async register(profile: Omit<UserProfile, 'id' | 'role'> & { password: string }): Promise<{ ok: boolean; message?: string }> {
    try {
      const payload = await firstValueFrom(
        this.http.post<unknown>('/api/auth/register', {
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email,
          phone: profile.phone,
          avatarUrl: profile.avatarUrl,
          preferredCurrency: profile.preferredCurrency ?? 'USD',
          password: profile.password
        })
      );

      const response = this.unwrap(payload) as AuthResponse;
      if (!response?.user?.id) {
        return { ok: false, message: 'Invalid registration response.' };
      }

      this.setUserProfile(this.mapUser(response.user));
      this.setWelcomeMessage(`Welcome ${response.user.firstName} ${response.user.lastName}`.trim());
      this.markOnline(response.user.email);
      return { ok: true };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  async requestPasswordReset(email: string): Promise<{ ok: boolean; message: string; resetToken?: string }> {
    try {
      const payload = await firstValueFrom(
        this.http.post<unknown>('/api/auth/password-reset/request', { email: email.trim() })
      );

      const response = this.unwrap(payload) as PasswordResetRequestResponse;
      return {
        ok: true,
        message: response?.message ?? 'If the account exists, reset instructions have been issued.',
        resetToken: response?.resetToken
      };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  async confirmPasswordReset(email: string, token: string, newPassword: string): Promise<{ ok: boolean; message?: string }> {
    try {
      await firstValueFrom(
        this.http.post<unknown>('/api/auth/password-reset/confirm', {
          email: email.trim(),
          token: token.trim(),
          newPassword: newPassword.trim()
        })
      );

      return { ok: true, message: 'Password has been reset. You can now sign in.' };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  async createSuperUser(profile: Omit<UserProfile, 'id' | 'role'> & { password: string }): Promise<{ ok: boolean; message?: string }> {
    try {
      const payload = this.unwrap(
        await firstValueFrom(
          this.http.post<unknown>('/api/accounts', {
            firstName: profile.firstName,
            lastName: profile.lastName,
            email: profile.email,
            phone: profile.phone,
            avatarUrl: profile.avatarUrl,
            preferredCurrency: profile.preferredCurrency ?? 'USD',
            role: 'super',
            password: profile.password
          })
        )
      ) as { success?: boolean; message?: string };

      this.refreshUsersCache();
      return { ok: payload?.success !== false, message: payload?.message };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  updateProfile(patch: Partial<UserProfile>): void {
    this.updateAccountSettings(patch).catch(() => undefined);
  }

  async updateAccountSettings(patch: Partial<UserProfile>): Promise<{ ok: boolean; message?: string }> {
    const current = this.user();
    if (!current) {
      return { ok: false, message: 'No active user session.' };
    }

    try {
      const payload = {
        firstName: patch.firstName ?? current.firstName,
        lastName: patch.lastName ?? current.lastName,
        email: (patch.email ?? current.email).trim(),
        phone: patch.phone ?? current.phone,
        avatarUrl: patch.avatarUrl ?? current.avatarUrl,
        preferredCurrency: patch.preferredCurrency ?? current.preferredCurrency ?? 'USD',
        roleId: current.roleId,
        role: current.role,
        isActive: patch.isActive ?? current.isActive ?? true,
        password: undefined as string | undefined
      };

      this.unwrap(await firstValueFrom(this.http.put<unknown>(`/api/accounts/${current.id}`, payload)));

      const merged: UserProfile = {
        ...current,
        ...patch,
        email: payload.email,
        preferredCurrency: payload.preferredCurrency as 'USD' | 'ZAR' | 'GBP'
      };

      this.setUserProfile(merged);
      if (current.email.toLowerCase() !== merged.email.toLowerCase()) {
        this.markOffline(current.email);
        this.markOnline(merged.email);
      }

      return { ok: true, message: 'Updated' };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<{ ok: boolean; message?: string }> {
    const current = this.user();
    if (!current) {
      return { ok: false, message: 'No active user session.' };
    }

    if (!currentPassword.trim() || !newPassword.trim()) {
      return { ok: false, message: 'Password fields are required.' };
    }

    if (newPassword.trim().length < 8) {
      return { ok: false, message: 'New password must be at least 8 characters.' };
    }

    try {
      const payload = {
        firstName: current.firstName,
        lastName: current.lastName,
        email: current.email,
        phone: current.phone,
        avatarUrl: current.avatarUrl,
        preferredCurrency: current.preferredCurrency ?? 'USD',
        roleId: current.roleId,
        role: current.role,
        isActive: current.isActive ?? true,
        password: newPassword.trim()
      };

      this.unwrap(await firstValueFrom(this.http.put<unknown>(`/api/accounts/${current.id}`, payload)));
      return { ok: true, message: 'Password updated.' };
    } catch (err: any) {
      return { ok: false, message: this.describeError(err) };
    }
  }

  logout(): void {
    const current = this.user();
    if (current?.email) {
      this.markOffline(current.email);
    }

    this.clearSession();
  }

  isAuthenticated(): boolean {
    return !!this.user();
  }

  isSuperUser(): boolean {
    const role = this.user()?.role?.toLowerCase();
    return role === 'super' || role === 'admin';
  }

  listUsers(): UserProfile[] {
    return this.usersCache();
  }

  listOnlineUsers(): UserProfile[] {
    const map = this.getOnlineMap();
    return this.usersCache().filter((u) => !!map[u.email.toLowerCase()]);
  }

  listPendingUsers(): UserProfile[] {
    return [];
  }

  consumeWelcomeMessage(): string {
    const message = this.welcomeMessage();
    this.welcomeMessage.set('');
    localStorage.removeItem(WELCOME_KEY);
    return message;
  }

  private restoreSession(): void {
    const rawUser = localStorage.getItem(AUTH_KEY);
    if (!rawUser) {
      return;
    }

    try {
      const user = JSON.parse(rawUser) as UserProfile;
      this.user.set(user);
      this.markOnline(user.email);
      const welcome = localStorage.getItem(WELCOME_KEY);
      if (welcome) {
        this.welcomeMessage.set(welcome);
      }

      this.refreshUsersCache();
    } catch {
      this.clearSession();
    }
  }

  private async refreshUsersCache(): Promise<void> {
    if (!this.isSuperUser()) {
      const current = this.user();
      this.usersCache.set(current ? [current] : []);
      return;
    }

    try {
      const payload = this.unwrap(await firstValueFrom(this.http.get<unknown>('/api/accounts?page=1&pageSize=200')));
      const data = (payload?.data?.items ?? payload?.items ?? payload?.data ?? []) as Array<any>;
      const users = data.map((item) => this.mapUser(item));
      this.usersCache.set(users);
    } catch {
      const current = this.user();
      this.usersCache.set(current ? [current] : []);
    }
  }

  private setUserProfile(profile: UserProfile): void {
    this.user.set(profile);
    localStorage.setItem(AUTH_KEY, JSON.stringify(profile));
  }

  private clearSession(): void {
    this.user.set(null);
    this.usersCache.set([]);
    localStorage.removeItem(AUTH_KEY);
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

  private unwrap(value: unknown): any {
    if (value === null || value === undefined || typeof value !== 'object') {
      return value;
    }

    const obj = value as Record<string, unknown>;
    if ('data' in obj) {
      return obj['data'];
    }

    return value;
  }

  private mapUser(input: any): UserProfile {
    const role = String(input?.role ?? 'regular').toLowerCase();
    const normalizedRole: UserRole = role === 'super' || role === 'admin' ? 'super' : 'regular';

    return {
      id: String(input?.id ?? ''),
      role: normalizedRole,
      roleId: Number(input?.roleId ?? 0) || undefined,
      firstName: String(input?.firstName ?? ''),
      lastName: String(input?.lastName ?? ''),
      email: String(input?.email ?? ''),
      phone: input?.phone ? String(input.phone) : undefined,
      avatarUrl: input?.avatarUrl ? String(input.avatarUrl) : undefined,
      preferredCurrency: (String(input?.preferredCurrency ?? 'USD').toUpperCase() as 'USD' | 'ZAR' | 'GBP') ?? 'USD',
      preferredLanguage: 'en',
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      emailNotifications: true,
      smsNotifications: false,
      marketingEmails: false,
      isActive: input?.isActive !== false
    };
  }

  private describeError(err: any): string {
    return (
      err?.error?.error?.detail ??
      err?.error?.error?.title ??
      err?.error?.message ??
      err?.error?.title ??
      err?.message ??
      'Request failed.'
    );
  }
}
