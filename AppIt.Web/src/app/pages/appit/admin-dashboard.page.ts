import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ChartModule } from 'primeng/chart';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService } from '../../core/api/api.service';
import { AdminStats } from '../../core/api/api.models';

@Component({
    standalone: true,
    imports: [CommonModule, ChartModule],
    template: `
        <section class="grid gap-6">
            <div class="flex flex-col gap-2">
                <p class="text-primary font-bold uppercase tracking-[0.25em]">Admin Overview</p>
                <h1 class="font-display text-4xl m-0">AppIt Control Center</h1>
                <p class="text-muted-color m-0">Live operational summary from the AppIt backend.</p>
            </div>

            <div *ngIf="loading()" class="flex justify-center py-8"><i class="pi pi-spin pi-spinner text-4xl text-primary"></i></div>

            <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4" *ngIf="!loading()">
                <article class="workspace-card" *ngFor="let card of cards()">
                    <div class="flex items-center justify-between">
                        <div>
                            <p class="text-sm text-muted-color m-0">{{ card.label }}</p>
                            <p class="text-3xl font-display font-bold mt-2 mb-0">{{ card.value }}</p>
                        </div>
                        <i [class]="card.icon + ' text-3xl text-primary'"></i>
                    </div>
                </article>
            </div>

            <div class="grid grid-cols-1 xl:grid-cols-2 gap-4">
                <article class="workspace-card">
                    <div class="flex items-center justify-between gap-3 mb-4">
                        <div>
                            <h2 class="font-display text-2xl mt-0 mb-1">Live Operational Summary</h2>
                            <p class="text-muted-color m-0">Sales, bookings, customers, and earnings in one view.</p>
                        </div>
                        <i class="pi pi-chart-bar text-2xl text-primary"></i>
                    </div>
                    <p-chart type="bar" [data]="operationalChart()" [options]="chartOptions" height="320px"></p-chart>
                </article>

                <article class="workspace-card">
                    <div class="flex items-center justify-between gap-3 mb-4">
                        <div>
                            <h2 class="font-display text-2xl mt-0 mb-1">Recent Data Health</h2>
                            <p class="text-muted-color m-0">Current record volume across active operational modules.</p>
                        </div>
                        <i class="pi pi-database text-2xl text-primary"></i>
                    </div>
                    <p-chart type="bar" [data]="healthChart()" [options]="chartOptions" height="320px"></p-chart>
                </article>
            </div>
            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `
})
export class AdminDashboardPage {
    private readonly api = inject(ApiService);
    readonly status = signal('');
    readonly loading = signal(false);
    readonly stats = signal<AdminStats>({});
    readonly health = signal<Array<{ label: string; count: number }>>([]);
    readonly cards = signal<Array<{ label: string; value: string; icon: string }>>([]);
    readonly operationalChart = signal<any>(this.emptyChart());
    readonly healthChart = signal<any>(this.emptyChart());
    readonly chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false }
        },
        scales: {
            x: { grid: { display: false } },
            y: { beginAtZero: true, ticks: { precision: 0 } }
        }
    };

    constructor() {
        this.load();
    }

    private load(): void {
        this.loading.set(true);
        this.status.set('');
        forkJoin({
            stats: this.api.get<AdminStats>('/api/admin/stats'),
            accounts: this.api.count('/api/accounts'),
            products: this.api.count('/api/products'),
            reservations: this.api.count('/api/reservations'),
            invoices: this.api.count('/api/invoices'),
            payments: this.api.count('/api/payments'),
            vouchers: this.api.count('/api/vouchers'),
            support: this.api.count('/api/support/messages')
        }).subscribe({
            next: ({ stats, ...rows }) => {
                this.stats.set(stats ?? {});
                const revenue = Number(stats?.totalEarnings ?? stats?.totalRevenue ?? 0);
                const bookings = Number(stats?.totalBookings ?? stats?.totalReservations ?? 0);
                const customers = Number(stats?.totalCustomers ?? stats?.totalAccounts ?? 0);
                const invoiceCount = Number(stats?.totalSales ?? rows.invoices ?? 0);
                this.cards.set([
                    { label: 'Revenue', value: this.money(revenue), icon: 'pi pi-wallet' },
                    { label: 'Bookings', value: String(bookings), icon: 'pi pi-calendar' },
                    { label: 'Customers', value: String(customers), icon: 'pi pi-users' },
                    { label: 'Invoices', value: String(invoiceCount), icon: 'pi pi-shopping-cart' }
                ]);
                this.operationalChart.set(this.barChart(
                    ['Revenue', 'Bookings', 'Customers', 'Invoices'],
                    [revenue, bookings, customers, invoiceCount],
                    ['#22c55e', '#14b8a6', '#f59e0b', '#2563eb']
                ));

                const health = [
                    { label: 'Accounts', count: rows.accounts },
                    { label: 'Products', count: rows.products },
                    { label: 'Reservations', count: rows.reservations },
                    { label: 'Invoices', count: rows.invoices },
                    { label: 'Payments', count: rows.payments },
                    { label: 'Vouchers', count: rows.vouchers },
                    { label: 'Support Messages', count: rows.support }
                ];
                this.health.set(health);
                this.healthChart.set(this.barChart(
                    health.map((item) => item.label),
                    health.map((item) => item.count),
                    ['#64748b', '#2563eb', '#14b8a6', '#f59e0b', '#22c55e', '#8b5cf6', '#ef4444']
                ));
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(this.describeError(err));
                this.loading.set(false);
            }
        });
    }

    private barChart(labels: string[], values: number[], colors: string[]): any {
        return {
            labels,
            datasets: [
                {
                    label: 'Records',
                    data: values,
                    backgroundColor: colors,
                    borderRadius: 8,
                    maxBarThickness: 54
                }
            ]
        };
    }

    private emptyChart(): any {
        return this.barChart([], [], []);
    }

    private money(value: number): string {
        return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value || 0);
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err, 'Dashboard sync failed.');
    }
}
