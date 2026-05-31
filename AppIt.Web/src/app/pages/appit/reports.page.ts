import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { ApiService } from '../../core/api/api.service';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { AuthService } from '../../core/auth/auth.service';

interface ReportDefinition {
    label: string;
    path: string;
    key: string;
    title: string;
    group: string;
    description: string;
}

const EMPTY_REPORT: ReportDefinition = {
    label: 'Report',
    path: '',
    key: 'report',
    title: 'Report Studio',
    group: 'Reports',
    description: 'Report definitions are loaded from the AppIt API.'
};

const REPORT_BRAND = {
    name: 'AppIt',
    tagline: 'Adventure and Hospitality Management Suite',
    email: 'admin@appit.com',
    phone: '+263 77 000 0000',
    address: 'Harare, Zimbabwe',
    poweredBy: 'Powered By Tedwell (YourItGuy - 2026)'
};

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, SelectModule, TableModule],
    template: `
        <section class="grid gap-5">
            <div class="workspace-card flex flex-col xl:flex-row xl:items-end xl:justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">Report Studio</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">{{ current().title }}</h1>
                    <p class="text-muted-color mt-2 mb-0">{{ current().description }}</p>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-[12rem_20rem_9rem_9rem] gap-2 w-full xl:w-auto">
                    <p-select [options]="groups()" [(ngModel)]="selectedGroup" (onChange)="applyGroup()" placeholder="Report group" appendTo="body"></p-select>
                    <p-select [options]="filteredReports()" [(ngModel)]="selectedKey" optionLabel="label" optionValue="key" (onChange)="applyReport()" placeholder="Report type" appendTo="body"></p-select>
                    <button pButton type="button" icon="pi pi-refresh" label="Generate" (click)="generate()"></button>
                    <button pButton type="button" icon="pi pi-file-pdf" label="PDF" class="app-pdf-button" (click)="exportPdf()" [disabled]="!rows().length"></button>
                </div>
            </div>

            <article class="workspace-card">
                <div class="report-filter-row">
                    <p-select [options]="periodOptions" [(ngModel)]="period" optionLabel="label" optionValue="value" appendTo="body" (onChange)="applyFilters()"></p-select>
                    <p-select [options]="bookingStatusOptions" [(ngModel)]="statusFilter" optionLabel="label" optionValue="value" appendTo="body" (onChange)="applyFilters()"></p-select>
                    <p-select [options]="agentOptions()" [(ngModel)]="agentFilter" optionLabel="label" optionValue="value" appendTo="body" (onChange)="applyFilters()"></p-select>
                    <p-select [options]="activityOptions()" [(ngModel)]="activityFilter" optionLabel="label" optionValue="value" appendTo="body" (onChange)="applyFilters()"></p-select>
                    <input *ngIf="period === 'day' || period === 'week'" pInputText type="date" [(ngModel)]="dateFilter" (change)="applyFilters()" />
                    <input *ngIf="period === 'month'" pInputText type="month" [(ngModel)]="dateFilter" (change)="applyFilters()" />
                    <input *ngIf="period === 'year'" pInputText type="number" min="1900" max="2100" step="1" [(ngModel)]="dateFilter" (change)="applyFilters()" placeholder="YYYY" />
                    <input pInputText type="date" [(ngModel)]="dateFrom" (change)="applyFilters()" placeholder="From" />
                    <input pInputText type="date" [(ngModel)]="dateTo" (change)="applyFilters()" placeholder="To" />
                    <button pButton type="button" icon="pi pi-filter-slash" label="Clear" severity="warn" (click)="clearFilters()"></button>
                    <button pButton type="button" icon="pi pi-table" label="All Reports" severity="info" (click)="generate()"></button>
                </div>
            </article>

            <article class="workspace-card overflow-x-auto">
                <div *ngIf="loading()" class="flex justify-center py-10"><i class="pi pi-spin pi-spinner text-4xl text-primary"></i></div>
                <p-table *ngIf="!loading()" [value]="rows()" [rows]="10" [paginator]="rows().length > 10" [rowsPerPageOptions]="[10]" responsiveLayout="scroll" styleClass="p-datatable-sm report-preview-table">
                    <ng-template pTemplate="header">
                        <tr>
                            <th *ngFor="let col of columns()">{{ label(col) }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td *ngFor="let col of columns()">{{ display(row[col]) }}</td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td [attr.colspan]="columns().length || 1" class="text-center text-muted-color py-6">Generate a report to preview data.</td>
                        </tr>
                    </ng-template>
                </p-table>
            </article>

            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `,
    styles: [`
        .report-filter-row {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            align-items: center;
        }

        .report-filter-row > * {
            min-width: 9.5rem;
        }

        :host ::ng-deep .report-preview-table .p-datatable-table {
            min-width: 100%;
        }

        :host ::ng-deep .report-preview-table th,
        :host ::ng-deep .report-preview-table td {
            white-space: nowrap;
        }
    `]
})
export class ReportsPage {
    private readonly api = inject(ApiService);
    private readonly auth = inject(AuthService);
    private readonly confirmations = inject(ConfirmationService);
    private readonly route = inject(ActivatedRoute);

    readonly reports = signal<ReportDefinition[]>([]);
    readonly groups = computed(() => Array.from(new Set(this.reports().map((report) => report.group))).sort());
    selectedGroup = 'Statistics';
    selectedKey = '';
    readonly current = signal<ReportDefinition>(EMPTY_REPORT);
    readonly rows = signal<any[]>([]);
    readonly sourceRows = signal<any[]>([]);
    readonly activities = signal<any[]>([]);
    readonly status = signal('');
    readonly loading = signal(false);
    period: 'day' | 'week' | 'month' | 'year' = 'month';
    statusFilter = 'All';
    agentFilter = 'All';
    activityFilter = 'All';
    dateFilter = '';
    dateFrom = '';
    dateTo = '';
    readonly periodOptions = [
        { label: 'Day', value: 'day' },
        { label: 'Week', value: 'week' },
        { label: 'Month', value: 'month' },
        { label: 'Year', value: 'year' }
    ];
    readonly bookingStatusOptions = [
        { label: 'All Booking Statuses', value: 'All' },
        { label: 'Pending', value: 'Pending' },
        { label: 'Confirmed', value: 'Confirmed' },
        { label: 'Completed', value: 'Completed' },
        { label: 'Open', value: 'Open' },
        { label: 'Cancelled', value: 'Cancelled' },
        { label: 'Closed', value: 'Closed' }
    ];

    constructor() {
        this.loadCatalog();
    }

    filteredReports(): ReportDefinition[] {
        return this.reports().filter((report) => report.group === this.selectedGroup);
    }

    applyGroup(): void {
        this.selectedKey = this.filteredReports()[0]?.key ?? '';
        this.applyReport();
    }

    applyReport(): void {
        this.current.set(this.reports().find((item) => item.key === this.selectedKey) ?? EMPTY_REPORT);
        this.statusFilter = this.statusFromReportKey(this.selectedKey);
        this.rows.set([]);
        this.sourceRows.set([]);
        this.status.set('');
    }

    generate(): void {
        const report = this.current();
        if (!report.path) {
            this.status.set('Select a report from the backend catalog first.');
            return;
        }

        this.loading.set(true);
        this.status.set('');
        this.api.list(report.path).subscribe({
            next: (rows) => {
                this.sourceRows.set(this.normalizeRows(rows));
                this.applyFilters();
                this.loadActivities();
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(this.describeError(err));
                this.loading.set(false);
            }
        });
    }

    applyFilters(): void {
        this.rows.set(this.sourceRows().filter((row) => {
            const status = String(row.status ?? '').toLowerCase();
            const date = this.rowDate(row);
            const services = this.servicesFor(row);
            const matchesStatus = this.statusFilter === 'All' || status.includes(this.statusFilter.toLowerCase());
            const matchesAgent = this.agentFilter === 'All' || String(row.accountId ?? 'Unassigned') === this.agentFilter;
            const matchesActivity = this.activityFilter === 'All' || services.some((item) => item.serviceType === 'Activity' && item.serviceName === this.activityFilter);
            return matchesStatus && matchesAgent && matchesActivity && this.matchesDateWindow(date);
        }));
    }

    clearFilters(): void {
        this.period = 'month';
        this.statusFilter = 'All';
        this.agentFilter = 'All';
        this.activityFilter = 'All';
        this.dateFilter = '';
        this.dateFrom = '';
        this.dateTo = '';
        this.applyFilters();
    }

    agentOptions(): Array<{ label: string; value: string }> {
        const agents = Array.from(new Set(this.sourceRows().map((row) => String(row.accountId ?? 'Unassigned'))));
        return [{ label: 'All Agents', value: 'All' }, ...agents.map((agent) => ({ label: `Agent ${agent}`, value: agent }))];
    }

    activityOptions(): Array<{ label: string; value: string }> {
        const names = new Set<string>();
        this.activities().forEach((item) => names.add(String(item.name ?? item.type ?? 'Activity')));
        this.sourceRows().flatMap((row) => this.servicesFor(row)).filter((item) => item.serviceType === 'Activity').forEach((item) => names.add(String(item.serviceName)));
        return [{ label: 'All Activities', value: 'All' }, ...Array.from(names).sort().map((name) => ({ label: name, value: name }))];
    }

    columns(): string[] {
        const row = this.rows()[0];
        return row ? Object.keys(row).filter((key) => typeof row[key] !== 'object').slice(0, 10) : [];
    }

    exportPdf(): void {
        if (this.rows().length > 100) {
            this.confirmations.confirm({
                header: 'Large PDF Export',
                message: `This export has ${this.rows().length} rows. The current PDF export includes the first 100 rows. Continue?`,
                icon: 'pi pi-exclamation-triangle',
                accept: () => this.writePdf()
            });
            return;
        }

        this.writePdf();
    }

    label(value: string): string {
        return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, (char) => char.toUpperCase());
    }

    display(value: unknown): string {
        if (value === null || value === undefined || value === '') return '-';
        if (typeof value === 'boolean') return value ? 'Yes' : 'No';
        if (typeof value === 'number') return Number.isInteger(value) ? String(value) : value.toFixed(2);
        return String(value);
    }

    private writePdf(): void {
        const columns = this.columns();
        const isWide = columns.length > 5;
        const doc = new jsPDF({ orientation: isWide ? 'landscape' : 'portrait', format: 'a4' });
        const pageWidth = doc.internal.pageSize.getWidth();
        const margin = 8;
        const fontSize = columns.length > 9 ? 5.5 : columns.length > 7 ? 6 : 7;
        const columnWidth = (pageWidth - (margin * 2)) / Math.max(1, columns.length);
        this.drawReportHeader(doc);
        autoTable(doc, {
            startY: 54,
            margin: { left: margin, right: margin },
            tableWidth: pageWidth - (margin * 2),
            head: [columns.map((column) => this.label(column))],
            body: this.rows().slice(0, 100).map((row) => columns.map((column) => this.display(row[column]))),
            styles: {
                fontSize,
                cellPadding: 1,
                overflow: 'linebreak',
                minCellHeight: 5,
                valign: 'middle'
            },
            columnStyles: Object.fromEntries(columns.map((_, index) => [index, { cellWidth: columnWidth }])),
            headStyles: { fillColor: [248, 179, 45], textColor: [15, 23, 42] }
        });
        doc.save(`${this.current().key}.pdf`);
    }

    private drawReportHeader(doc: jsPDF): void {
        const pageWidth = doc.internal.pageSize.getWidth();
        doc.setFillColor(248, 179, 45);
        doc.rect(0, 0, pageWidth, 28, 'F');

        doc.setFillColor(15, 23, 42);
        doc.roundedRect(12, 7, 14, 14, 2, 2, 'F');
        doc.setTextColor(248, 179, 45);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(11);
        doc.text('A', 17, 17);

        doc.setTextColor(15, 23, 42);
        doc.setFontSize(15);
        doc.text(REPORT_BRAND.name, 31, 13);
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8);
        doc.text(REPORT_BRAND.tagline, 31, 19);

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(12);
        doc.text(this.current().title, 12, 36);
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8);
        doc.text(`Report group: ${this.current().group}`, 12, 42);
        doc.text(`Generated by ${this.auth.displayName()} on ${new Date().toLocaleString()}`, 12, 47);

        const contact = `${REPORT_BRAND.email} | ${REPORT_BRAND.phone} | ${REPORT_BRAND.address}`;
        doc.text(contact, pageWidth - 12, 36, { align: 'right' });
        doc.text(REPORT_BRAND.poweredBy, pageWidth - 12, 42, { align: 'right' });
    }

    private normalizeRows(value: unknown): any[] {
        if (Array.isArray(value)) return value;
        if (value && typeof value === 'object') return [value];
        return [];
    }

    private loadCatalog(): void {
        const routeGroup = this.route.snapshot.data['reportGroup'] as string | undefined;
        this.api.list<ReportDefinition>('/api/frontend/report-catalog').subscribe({
            next: (reports) => {
                this.reports.set(reports);
                this.selectedGroup = this.groups().includes(routeGroup ?? '') ? routeGroup! : this.groups()[0] ?? 'Reports';
                this.selectedKey = this.filteredReports()[0]?.key ?? '';
                this.applyReport();
            },
            error: (err) => {
                this.status.set(this.describeError(err));
                this.reports.set([]);
                this.current.set(EMPTY_REPORT);
            }
        });
    }

    private loadActivities(): void {
        this.api.list('/api/activities').subscribe({
            next: (rows) => this.activities.set(rows),
            error: () => this.activities.set([])
        });
    }

    private rowDate(row: any): Date {
        return new Date(row.createdAt ?? row.createdDate ?? row.issuedDate ?? row.processedAt ?? Date.now());
    }

    private parseDateFilter(): Date {
        if (!this.dateFilter) return new Date();
        const filter = String(this.dateFilter);
        const date = new Date(`${filter}T00:00:00`);
        if (!isNaN(date.getTime())) return date;
        const parts = filter.split('-');
        if (parts.length === 2) return new Date(Number(parts[0]), Number(parts[1]) - 1);
        return new Date(Number(filter), 0);
    }

    private matchesDateWindow(date: Date): boolean {
        if (Number.isNaN(date.getTime())) return true;
        if (this.dateFrom && date < new Date(`${this.dateFrom}T00:00:00`)) return false;
        if (this.dateTo && date > new Date(`${this.dateTo}T23:59:59`)) return false;
        if (!this.dateFilter) return true;

        const selected = this.parseDateFilter();
        if (this.period === 'day') return this.sameDay(date, selected);
        if (this.period === 'week') return this.weekStart(date).getTime() === this.weekStart(selected).getTime();
        if (this.period === 'year') return date.getFullYear() === selected.getFullYear();
        return date.getFullYear() === selected.getFullYear() && date.getMonth() === selected.getMonth();
    }

    private sameDay(a: Date, b: Date): boolean {
        return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
    }

    private weekStart(value: Date): Date {
        const date = new Date(value.getFullYear(), value.getMonth(), value.getDate());
        date.setDate(date.getDate() - date.getDay());
        return date;
    }

    private servicesFor(row: any): any[] {
        return Array.isArray(row.serviceItems) ? row.serviceItems : [];
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err, 'Report request failed.');
    }

    private statusFromReportKey(key: string): string {
        if (key.includes('pending')) return 'Pending';
        if (key.includes('completed')) return 'Completed';
        if (key.includes('open')) return 'Open';
        if (key.includes('cancelled')) return 'Cancelled';
        return 'All';
    }
}
