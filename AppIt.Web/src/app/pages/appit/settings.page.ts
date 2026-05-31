import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
        <section class="grid gap-5">
            <div>
                <p class="text-primary font-bold uppercase tracking-[0.25em]">Settings</p>
                <h1 class="font-display text-4xl mt-2 mb-0">Account Profile</h1>
            </div>
            <form class="workspace-card grid gap-4 max-w-3xl" (ngSubmit)="save()">
                <div class="grid md:grid-cols-2 gap-4">
                    <label class="grid gap-2">First name<input class="p-inputtext p-component" [(ngModel)]="profile.firstName" name="firstName" /></label>
                    <label class="grid gap-2">Last name<input class="p-inputtext p-component" [(ngModel)]="profile.lastName" name="lastName" /></label>
                    <label class="grid gap-2">Email<input class="p-inputtext p-component" [(ngModel)]="profile.email" name="email" /></label>
                    <label class="grid gap-2">Phone<input class="p-inputtext p-component" [(ngModel)]="profile.phone" name="phone" /></label>
                </div>
                <button class="p-button p-component w-fit" type="submit" [disabled]="saving()">
                    <i class="pi pi-spin pi-spinner mr-2" *ngIf="saving()"></i>
                    <span class="p-button-label">{{ saving() ? 'Saving...' : 'Save Profile' }}</span>
                </button>
                <p class="text-primary" *ngIf="status()">{{ status() }}</p>
            </form>
        </section>
    `
})
export class SettingsPage {
    private readonly api = inject(ApiService);
    private readonly auth = inject(AuthService);
    readonly status = signal('');
    readonly saving = signal(false);
    profile = { ...(this.auth.user() ?? {}) } as any;

    save(): void {
        const id = this.auth.user()?.id;
        if (!id) return;
        this.saving.set(true);
        this.status.set('');
        this.api.put(`/api/accounts/${id}`, this.profile).subscribe({
            next: (updated: any) => {
                this.saving.set(false);
                this.status.set('Profile saved successfully.');
                const data = updated?.data ?? updated;
                if (data) {
                    this.auth.patchUser(data);
                }
            },
            error: (err) => {
                this.saving.set(false);
                this.status.set(err?.error?.message ?? err?.message ?? 'Profile update failed.');
            }
        });
    }
}
