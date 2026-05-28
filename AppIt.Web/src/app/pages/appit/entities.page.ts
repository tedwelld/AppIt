import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService, SYSTEM_PAGE_SIZE } from '../../core/api/api.service';
import { APPIT_ENTITIES } from '../../core/navigation/workspace-navigation';

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, DialogModule, InputTextModule, TableModule],
    template: `
        <section class="grid gap-5">
            <div class="flex flex-col md:flex-row md:items-end md:justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">{{ config()?.group || 'Entity' }} Manager</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">{{ config()?.title || 'Entity' }}</h1>
                    <p class="text-muted-color mt-2 mb-0">{{ resource() === 'trip-accounts' ? 'Create walk-in agents and company agents that receive services from the business.' : 'Create, view, edit, and hard-delete records from one table.' }}</p>
                </div>
                <div class="flex flex-col sm:flex-row gap-2 w-full md:w-auto">
                    <input pInputText class="w-full md:w-80" [(ngModel)]="query" (keyup.enter)="load(1)" placeholder="Search rows..." />
                    <button pButton type="button" icon="pi pi-search" label="Search" severity="secondary" (click)="load(1)"></button>
                    <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="secondary" (click)="load(currentPage())"></button>
                    <button pButton type="button" icon="pi pi-plus" [label]="resource() === 'trip-accounts' ? 'New Agent' : 'New'" (click)="openCreate()" [disabled]="config()?.readonly"></button>
                </div>
            </div>

            <article class="workspace-card">
                <p-table
                    [value]="rows()"
                    [lazy]="true"
                    [paginator]="true"
                    [rows]="pageSize"
                    [totalRecords]="totalRecords()"
                    [first]="first()"
                    [loading]="loading()"
                    [rowsPerPageOptions]="[10]"
                    dataKey="id"
                    responsiveLayout="scroll"
                    (onLazyLoad)="onLazyLoad($event)"
                    styleClass="p-datatable-sm"
                >
                    <ng-template pTemplate="header">
                        <tr>
                            <th *ngFor="let column of columns()">{{ label(column) }}</th>
                            <th class="text-right w-36">Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td *ngFor="let column of columns()">{{ display(row[column]) }}</td>
                            <td class="text-right">
                                <div class="flex justify-end gap-2">
                                    <button pButton type="button" icon="pi pi-pencil" severity="secondary" rounded text aria-label="Edit row" (click)="openEdit(row)"></button>
                                    <button pButton type="button" icon="pi pi-trash" severity="danger" rounded text aria-label="Hard delete row" (click)="confirmDelete(row)" [disabled]="!rowId(row)"></button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td [attr.colspan]="columns().length + 1" class="text-center text-muted-color py-6">No rows loaded.</td>
                        </tr>
                    </ng-template>
                </p-table>
            </article>

            <p-dialog
                [header]="dialogMode() === 'create' ? 'Create ' + (config()?.title || 'Record') : 'Edit ' + (config()?.title || 'Record')"
                [(visible)]="dialogVisible"
                [modal]="true"
                [style]="{ width: 'min(720px, 94vw)' }"
                [draggable]="false"
            >
                <form class="grid grid-cols-1 md:grid-cols-2 gap-4" (ngSubmit)="save()">
                    <label class="grid gap-2" *ngFor="let field of formFields()">
                        <span class="font-semibold">{{ label(field) }}</span>
                        <input
                            *ngIf="fieldType(field) !== 'boolean'"
                            pInputText
                            [type]="fieldType(field)"
                            [(ngModel)]="draft[field]"
                            [name]="field"
                        />
                        <select *ngIf="fieldType(field) === 'boolean'" class="p-inputtext p-component" [(ngModel)]="draft[field]" [name]="field">
                            <option [ngValue]="true">True</option>
                            <option [ngValue]="false">False</option>
                        </select>
                    </label>
                    <div class="md:col-span-2 flex justify-end gap-2 pt-2">
                        <button pButton type="button" label="Cancel" severity="secondary" (click)="dialogVisible = false"></button>
                        <button pButton type="submit" icon="pi pi-save" label="Save" [loading]="saving()"></button>
                    </div>
                </form>
            </p-dialog>

            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `
})
export class EntitiesPage {
    private readonly api = inject(ApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly confirmations = inject(ConfirmationService);
    private readonly messages = inject(MessageService);
    readonly rows = signal<any[]>([]);
    readonly totalRecords = signal(0);
    readonly first = signal(0);
    readonly loading = signal(false);
    readonly saving = signal(false);
    readonly status = signal('');
    readonly resource = signal('');
    readonly dialogMode = signal<'create' | 'edit'>('create');
    readonly pageSize = SYSTEM_PAGE_SIZE;
    query = '';
    dialogVisible = false;
    draft: Record<string, any> = {};

    readonly config = computed(() => APPIT_ENTITIES.find((item) => item.key === this.resource()));
    readonly columns = computed(() => {
        const firstRow = this.rows()[0];
        const keys = firstRow ? Object.keys(firstRow) : this.defaultFields();
        return keys.filter((key) => typeof firstRow?.[key] !== 'object').slice(0, 8);
    });
    readonly formFields = computed(() => {
        const row = this.rows()[0] ?? {};
        return this.defaultFields()
            .concat(Object.keys(row))
            .filter((field, index, all) => all.indexOf(field) === index)
            .filter((field) => !this.isSystemField(field) && typeof row[field] !== 'object');
    });

    constructor() {
        this.route.paramMap.subscribe((params) => {
            this.resource.set(params.get('resource') ?? this.route.snapshot.data['resource'] ?? 'accounts');
            this.query = '';
            this.load(1);
        });
    }

    onLazyLoad(event: any): void {
        const page = Math.floor((event.first ?? 0) / this.pageSize) + 1;
        this.first.set(event.first ?? 0);
        this.load(page);
    }

    load(page = 1): void {
        const config = this.config();
        if (!config) {
            this.rows.set([]);
            this.totalRecords.set(0);
            this.status.set('Unknown resource.');
            return;
        }

        this.loading.set(true);
        this.api.listPage(config.endpoint, page, this.pageSize, this.query).subscribe({
            next: (result) => {
                this.rows.set(result.items ?? []);
                this.totalRecords.set(result.totalCount ?? result.items?.length ?? 0);
                this.first.set(((result.page ?? page) - 1) * this.pageSize);
                this.status.set('');
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(this.describeError(err));
                this.loading.set(false);
            }
        });
    }

    currentPage(): number {
        return Math.floor(this.first() / this.pageSize) + 1;
    }

    openCreate(): void {
        this.dialogMode.set('create');
        this.draft = this.formFields().reduce((acc, field) => ({ ...acc, [field]: this.defaultValue(field) }), {});
        this.dialogVisible = true;
    }

    openEdit(row: any): void {
        this.dialogMode.set('edit');
        this.draft = { ...row };
        this.dialogVisible = true;
    }

    save(): void {
        const config = this.config();
        if (!config) return;

        const payload = this.cleanPayload(this.draft);
        this.saving.set(true);
        const request = this.dialogMode() === 'create'
            ? this.api.post(config.endpoint, payload)
            : this.api.put(`${config.endpoint}/${this.rowId(this.draft)}`, payload);

        request.subscribe({
            next: () => {
                this.saving.set(false);
                this.dialogVisible = false;
                this.messages.add({ severity: 'success', summary: 'Saved', detail: `${config.title} record saved.` });
                this.load(Math.floor(this.first() / this.pageSize) + 1);
            },
            error: (err) => {
                this.saving.set(false);
                this.status.set(this.describeError(err));
            }
        });
    }

    confirmDelete(row: any): void {
        const config = this.config();
        const id = this.rowId(row);
        if (!config || !id) return;

        this.confirmations.confirm({
            header: 'Hard Delete Record',
            message: `Hard delete ${config.title} #${id}? This permanently removes the row.`,
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => this.remove(row)
        });
    }

    remove(row: any): void {
        const config = this.config();
        const id = this.rowId(row);
        if (!config || !id) return;

        this.api.delete(`${config.endpoint}/${id}`).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Deleted', detail: `${config.title} #${id} was hard deleted.` });
                this.load(Math.floor(this.first() / this.pageSize) + 1);
            },
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    rowId(row: any): string | number | null {
        const fields = this.config()?.idFields ?? ['id'];
        for (const field of fields) {
            if (row?.[field] !== undefined && row?.[field] !== null && row?.[field] !== '') return row[field];
        }
        return null;
    }

    display(value: unknown): string {
        if (value === null || value === undefined || value === '') return '-';
        if (typeof value === 'boolean') return value ? 'Yes' : 'No';
        return String(value);
    }

    label(value: string): string {
        return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, (char) => char.toUpperCase());
    }

    fieldType(field: string): string {
        const value = this.draft[field];
        if (typeof value === 'boolean' || field.toLowerCase().startsWith('is')) return 'boolean';
        if (typeof value === 'number' || /amount|price|capacity|id$/i.test(field)) return 'number';
        if (/date|at$/i.test(field)) return 'datetime-local';
        if (field.toLowerCase().includes('email')) return 'email';
        if (field.toLowerCase().includes('password')) return 'password';
        return 'text';
    }

    private cleanPayload(row: Record<string, any>): Record<string, any> {
        return Object.fromEntries(
            Object.entries(row)
                .filter(([key, value]) => !this.isReadOnlyField(key) && value !== undefined)
                .map(([key, value]) => [key, value === '' ? null : value])
        );
    }

    private defaultFields(): string[] {
        switch (this.resource()) {
            case 'accounts':
                return ['firstName', 'lastName', 'email', 'password', 'phone', 'preferredCurrency', 'role', 'roleId', 'isActive'];
            case 'roles':
                return ['name'];
            case 'features':
            case 'departments':
                return ['name', 'description'];
            case 'permissions':
                return ['name'];
            case 'companies':
                return ['companyName', 'companyEmail', 'companyPhone', 'regNumber'];
            case 'trip-accounts':
                return ['agentName', 'agentType', 'email', 'phone', 'address', 'registrationNumber', 'accountNumber', 'vatNumber'];
            case 'suppliers':
                return ['name', 'contactEmail', 'contactPhone', 'description'];
            case 'products':
                return ['name', 'category', 'description', 'basePriceUsd', 'isActive'];
            case 'accommodations':
                return ['type', 'description', 'capacity', 'basePriceUsd', 'isActive'];
            case 'activities':
                return ['name', 'description', 'basePriceUsd', 'isActive'];
            case 'reservations':
                return ['reference', 'voucherCode', 'customerId', 'accountId', 'currency', 'totalAmount', 'status', 'customerEmail'];
            case 'invoices':
                return ['reservationId', 'totalAmount', 'currency', 'status'];
            case 'payments':
                return ['invoiceId', 'method', 'status', 'transactionReference', 'amount', 'currencyCode', 'processedAt'];
            case 'vouchers':
                return ['code', 'reference', 'type', 'comboReference', 'reservationId'];
            case 'support-messages':
                return ['fromEmail', 'toEmail', 'subject', 'message', 'status'];
            case 'currencies':
                return ['name', 'code', 'isActive'];
            default:
                return ['name', 'status'];
        }
    }

    private defaultValue(field: string): any {
        if (field === 'preferredCurrency' || field === 'currency' || field === 'currencyCode') return 'USD';
        if (field === 'agentType') return 'Company';
        if (field === 'role') return 'regular';
        if (field === 'status') return this.resource() === 'support-messages' ? 'Open' : 'Pending';
        if (field === 'method') return 'Manual';
        if (field === 'type') return 'Reservation';
        if (field.toLowerCase().startsWith('is')) return true;
        if (/amount|price|capacity|id$/i.test(field)) return null;
        return '';
    }

    private isSystemField(field: string): boolean {
        return this.isReadOnlyField(field) || (this.config()?.idFields ?? []).some((idField) => idField.toLowerCase() === field.toLowerCase());
    }

    private isReadOnlyField(field: string): boolean {
        return /created|updated|issued|processed/i.test(field) && field !== 'processedAt';
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err, 'Entity request failed.');
    }
}
