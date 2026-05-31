import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
        <section class="grid gap-6">
            <div class="workspace-card metric-gradient">
                <p class="text-primary font-bold uppercase tracking-[0.25em]">Guest Portal</p>
                <h1 class="font-display text-4xl mt-2 mb-2">Welcome, {{ auth.displayName() }}</h1>
                <p class="text-muted-color m-0">Browse stays, activities, reservations, invoices, payments, and vouchers.</p>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <article class="workspace-card" *ngFor="let card of cards()">
                    <p class="text-muted-color text-sm m-0">{{ card.label }}</p>
                    <p class="text-3xl font-display font-bold mt-2 mb-0">{{ card.value }}</p>
                </article>
            </div>

            <article class="workspace-card">
                <h2 class="font-display text-2xl mt-0">Available Experiences</h2>
                <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                    <div *ngFor="let item of catalog()" class="p-4 rounded-xl border border-surface-200 dark:border-surface-700">
                        <p class="font-bold text-lg m-0">{{ item.name || item.title || item.productName || 'Experience' }}</p>
                        <p class="text-muted-color text-sm mt-2 mb-0">{{ item.description || item.type || item.category || 'Ready to reserve' }}</p>
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

    constructor() {
        const accountId = this.auth.user()?.id;
        forkJoin({
            reservations: this.api.list('/api/reservations/mine'),
            invoices: this.api.list('/api/invoices/mine'),
            vouchers: this.api.list('/api/vouchers/mine'),
            products: this.api.list('/api/products'),
            accommodations: this.api.list('/api/accommodations'),
            activities: this.api.list('/api/activities')
        }).subscribe({
            next: (rows) => {
                this.cards.set([
                    { label: 'Reservations', value: rows.reservations.length },
                    { label: 'Invoices', value: rows.invoices.length },
                    { label: 'Vouchers', value: rows.vouchers.length }
                ]);
                this.catalog.set([...rows.products, ...rows.accommodations, ...rows.activities].slice(0, 12));
            },
            error: (err) => this.status.set(err?.error?.message ?? err?.message ?? 'Dashboard data request failed.')
        });
    }
}
