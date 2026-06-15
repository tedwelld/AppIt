import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
        <section class="grid gap-6">
            <div class="workspace-card metric-gradient">
                <p class="text-primary font-bold uppercase tracking-[0.25em]">Guest Portal</p>
                <h1 class="font-display text-4xl mt-2 mb-2">Welcome, {{ auth.displayName() }}</h1>
                <p class="text-muted-color m-0">Browse stays, activities, reservations, invoices, payments, and vouchers.</p>
                <a routerLink="/user/bookings/new" class="inline-flex mt-4 px-4 py-2 rounded-lg bg-primary text-primary-contrast font-semibold no-underline">Book Now</a>
            </div>

            <div *ngIf="loading()" class="flex justify-center py-8"><i class="pi pi-spin pi-spinner text-4xl text-primary"></i></div>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4" *ngIf="!loading()">
                <article class="workspace-card" *ngFor="let card of cards()">
                    <p class="text-muted-color text-sm m-0">{{ card.label }}</p>
                    <p class="text-3xl font-display font-bold mt-2 mb-0">{{ card.value }}</p>
                </article>
            </div>

            <article class="workspace-card" *ngIf="!loading()">
                <h2 class="font-display text-2xl mt-0">Available Experiences</h2>
                <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                    <div *ngFor="let item of catalog()" class="p-4 rounded-xl border border-surface-200 dark:border-surface-700">
                        <p class="font-bold text-lg m-0">{{ item.name || item.title || item.productName || 'Experience' }}</p>
                        <p class="text-muted-color text-sm mt-2 mb-3">{{ item.description || item.type || item.category || 'Ready to reserve' }}</p>
                        <a routerLink="/user/bookings/new" class="text-primary font-semibold no-underline">Book this experience</a>
                    </div>
                </div>
            </article>
            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `
})
export class UserDashboardPage {
    private readonly api = inject(ApiService);
    readonly auth = inject(AuthService);
    readonly cards = signal<Array<{ label: string; value: number }>>([]);
    readonly catalog = signal<any[]>([]);
    readonly status = signal('');
    readonly loading = signal(true);

    constructor() {
        const accountId = this.auth.user()?.id;
        const mine = accountId ? `?accountId=${accountId}` : '';
        forkJoin({
            reservations: this.api.listAll(`/api/reservations/mine${mine}`),
            invoices: this.api.listAll(`/api/invoices/mine${mine}`),
            vouchers: this.api.listAll(`/api/vouchers/mine${mine}`),
            products: this.api.listAll('/api/products'),
            accommodations: this.api.listAll('/api/accommodations'),
            activities: this.api.listAll('/api/activities')
        }).subscribe({
            next: (rows) => {
                this.cards.set([
                    { label: 'Reservations', value: rows.reservations.length },
                    { label: 'Invoices', value: rows.invoices.length },
                    { label: 'Vouchers', value: rows.vouchers.length }
                ]);
                this.catalog.set([...rows.products, ...rows.accommodations, ...rows.activities].slice(0, 12));
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(extractApiErrorMessage(err?.error ?? err, 'Dashboard data request failed.'));
                this.loading.set(false);
            }
        });
    }
}
