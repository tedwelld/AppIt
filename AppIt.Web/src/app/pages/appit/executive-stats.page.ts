import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartModule } from 'primeng/chart';
import { SelectModule } from 'primeng/select';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService } from '../../core/api/api.service';
import { AdminStats } from '../../core/api/api.models';

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule, ChartModule, SelectModule],
    template: `
        <section class="grid gap-6">
            <div class="workspace-card flex flex-col md:flex-row md:items-end md:justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">Executive Statistics</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">Leadership Dashboard</h1>
                    <p class="text-muted-color m-0">Trend and revenue view for management reporting.</p>
                </div>
                <p-select [options]="rangeOptions" [(ngModel)]="range" optionLabel="label" optionValue="value" (onChange)="load()" appendTo="body"></p-select>
            </div>

            <div *ngIf="loading()" class="flex justify-center py-8"><i class="pi pi-spin pi-spinner text-4xl text-primary"></i></div>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4" *ngIf="!loading()">
                <article class="workspace-card" *ngFor="let card of summaryCards()">
                    <p class="text-muted-color text-sm m-0">{{ card.label }}</p>
                    <p class="text-3xl font-display font-bold mt-2 mb-0">{{ card.value }}</p>
                </article>
            </div>

            <article class="workspace-card" *ngIf="!loading()">
                <h2 class="font-display text-2xl mt-0">Revenue Trend</h2>
                <p-chart type="line" [data]="trendChart()" [options]="chartOptions" height="360px"></p-chart>
            </article>
            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `
})
export class ExecutiveStatsPage {
    private readonly api = inject(ApiService);
    readonly status = signal('');
    readonly loading = signal(false);
    readonly summaryCards = signal<Array<{ label: string; value: string }>>([]);
    readonly trendChart = signal<any>({ labels: [], datasets: [] });
    range: '7d' | '30d' | '90d' = '30d';
    readonly rangeOptions = [
        { label: 'Last 7 days', value: '7d' },
        { label: 'Last 30 days', value: '30d' },
        { label: 'Last 90 days', value: '90d' }
    ];
    readonly chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true } }
    };

    constructor() {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.api.get<AdminStats>(`/api/admin/stats?range=${this.range}`).subscribe({
            next: (stats) => {
                const revenue = Number(stats?.totalEarnings ?? stats?.totalRevenue ?? 0);
                const bookings = Number(stats?.totalBookings ?? stats?.totalReservations ?? 0);
                const customers = Number(stats?.totalCustomers ?? stats?.totalAccounts ?? 0);
                this.summaryCards.set([
                    { label: 'Revenue', value: this.money(revenue) },
                    { label: 'Bookings', value: String(bookings) },
                    { label: 'Customers', value: String(customers) }
                ]);

                const trend = stats?.trend ?? (stats?.['Trend'] as AdminStats['trend']) ?? [];
                this.trendChart.set({
                    labels: trend.map((point, index) => point.label ?? `Day ${index + 1}`),
                    datasets: [{
                        label: 'Revenue',
                        data: trend.map((point) => Number(point.value ?? 0)),
                        borderColor: '#2563eb',
                        backgroundColor: 'rgba(37, 99, 235, 0.15)',
                        fill: true,
                        tension: 0.35
                    }]
                });
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(extractApiErrorMessage(err?.error ?? err, 'Failed to load executive stats.'));
                this.loading.set(false);
            }
        });
    }

    private money(value: number): string {
        return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value || 0);
    }
}
