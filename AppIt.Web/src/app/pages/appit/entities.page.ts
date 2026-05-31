import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { forkJoin } from 'rxjs';
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
                    <button pButton type="button" icon="pi pi-search" label="Search" severity="info" (click)="load(1)"></button>
                    <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="warn" (click)="load(currentPage())"></button>
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
                                    <button pButton type="button" icon="pi pi-pencil" severity="info" rounded text aria-label="Edit row" (click)="openEdit(row)"></button>
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
                        <select *ngIf="selectOptions(field)" class="p-inputtext p-component" [(ngModel)]="draft[field]" [name]="field" [disabled]="isLockedServiceCategoryField(field)" (ngModelChange)="onDraftFieldChange(field)">
                            <option *ngFor="let opt of selectOptions(field)!" [value]="opt">{{ opt }}</option>
                        </select>
                        <select *ngIf="!selectOptions(field) && fieldType(field) === 'boolean'" class="p-inputtext p-component" [(ngModel)]="draft[field]" [name]="field">
                            <option [ngValue]="true">Yes</option>
                            <option [ngValue]="false">No</option>
                        </select>
                        <input
                            *ngIf="!selectOptions(field) && fieldType(field) !== 'boolean'"
                            pInputText
                            [type]="fieldType(field)"
                            [(ngModel)]="draft[field]"
                            [name]="field"
                            [readonly]="isLockedServiceCategoryField(field)"
                        />
                    </label>
                    <div class="md:col-span-2 flex justify-end gap-2 pt-2">
                        <button pButton type="button" label="Cancel" severity="contrast" (click)="dialogVisible = false"></button>
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
    readonly roleNames = signal<string[]>([]);
    readonly pageSize = SYSTEM_PAGE_SIZE;
    query = '';
    dialogVisible = false;
    draft: Record<string, any> = {};

    readonly config = computed(() => APPIT_ENTITIES.find((item) => item.key === this.resource()));
    readonly columns = computed(() => {
        if (this.resource() === 'products') {
            return ['serviceTypeCategory', 'name', 'description', 'capacity', 'guestCapacity', 'basePriceUsd', 'isActive'];
        }
        const firstRow = this.rows()[0];
        const keys = firstRow ? Object.keys(firstRow) : this.defaultFields();
        return keys.filter((key) => typeof firstRow?.[key] !== 'object').slice(0, 8);
    });
    readonly formFields = computed(() => {
        const row = this.rows()[0] ?? {};
        const baseFields = this.resource() === 'products' ? this.productFieldsForDraft() : this.defaultFields();
        const fields = baseFields
            .concat(Object.keys(row))
            .filter((field, index, all) => all.indexOf(field) === index)
            .filter((field) => !(this.resource() === 'products' && field.toLowerCase() === 'category'))
            .filter((field) => !(this.resource() === 'products' && ['id', 'sourceId', 'sourceEndpoint'].includes(field)))
            .filter((field) => !this.isSystemField(field) && typeof row[field] !== 'object');
        const serviceType = this.serviceTypeForResource();
        return (serviceType || this.resource() === 'products') && !fields.includes('serviceTypeCategory')
            ? ['serviceTypeCategory', ...fields]
            : fields;
    });

    constructor() {
        this.route.paramMap.subscribe((params) => {
            this.resource.set(params.get('resource') ?? this.route.snapshot.data['resource'] ?? 'accounts');
            this.query = '';
            this.load(1);
            if (this.resource() === 'accounts') {
                this.loadRoleNames();
            }
        });
    }

    private loadRoleNames(): void {
        this.api.list<any>('/api/roles').subscribe({
            next: (roles) => this.roleNames.set(
                (roles ?? [])
                    .map((r: any) => String(r.name ?? r.roleName ?? '').trim())
                    .filter((name: string) => !!name)
            ),
            error: () => this.roleNames.set([])
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

        if (this.resource() === 'products') {
            this.loadUnifiedProducts(page);
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
        if (this.resource() === 'products') {
            this.draft['serviceTypeCategory'] = 'Product';
            this.syncProductDraftFields();
        }
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

        if (this.resource() === 'products') {
            this.saveUnifiedProduct();
            return;
        }

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

        if (this.resource() === 'products') {
            const endpoint = this.endpointForServiceType(row.serviceTypeCategory);
            this.api.delete(`${endpoint}/${id}`).subscribe({
                next: () => {
                    this.messages.add({ severity: 'success', summary: 'Deleted', detail: `${row.serviceTypeCategory} #${id} was hard deleted.` });
                    this.load(Math.floor(this.first() / this.pageSize) + 1);
                },
                error: (err) => this.status.set(this.describeError(err))
            });
            return;
        }

        this.api.delete(`${config.endpoint}/${id}`).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Deleted', detail: `${config.title} #${id} was hard deleted.` });
                this.load(Math.floor(this.first() / this.pageSize) + 1);
            },
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    rowId(row: any): string | number | null {
        if (this.resource() === 'products' && row?.sourceId) return row.sourceId;
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
        if (['category', 'serviceType', 'serviceTypeCategory', 'productType'].includes(value)) return 'Service Type / Category';
        return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, (char) => char.toUpperCase());
    }

    isLockedServiceCategoryField(field: string): boolean {
        return field === 'serviceTypeCategory';
    }

    fieldType(field: string): string {
        const value = this.draft[field];
        if (typeof value === 'boolean' || field.toLowerCase().startsWith('is')) return 'boolean';
        if (typeof value === 'number' || /amount|price|capacity|rate|percentage|id$/i.test(field)) return 'number';
        if (/date|at$/i.test(field)) return 'datetime-local';
        if (field.toLowerCase().includes('email')) return 'email';
        if (field.toLowerCase().includes('password')) return 'password';
        return 'text';
    }

    selectOptions(field: string): string[] | null {
        const f = field.toLowerCase();
        if (f === 'role') {
            const loaded = this.roleNames();
            return loaded.length ? loaded : ['regular', 'admin', 'super'];
        }
        if (f === 'agenttype') return ['Company', 'Individual', 'Agency'];
        if (f === 'method') return ['Manual', 'Cash', 'Card', 'Bank Transfer', 'Stripe', 'PayPal'];
        if (f === 'servicetypecategory') {
            if (this.resource() === 'products') return ['Product', 'Accommodation', 'Activity', 'Transfer', 'Tour'];
            const serviceType = this.serviceTypeForResource();
            return serviceType ? [serviceType] : ['Product', 'Accommodation', 'Activity', 'Transfer', 'Tour'];
        }
        if (f === 'producttype' || f === 'servicetype') return ['Product', 'Accommodation', 'Activity', 'Transfer', 'Tour'];
        if (f === 'type') {
            if (this.resource() === 'vouchers') return ['Reservation', 'Standard', 'Group'];
            if (this.resource() === 'special-product-prices') return ['Product', 'Accommodation', 'Activity', 'Transfer', 'Tour'];
            return ['Reservation', 'Standard', 'Group', 'Corporate'];
        }
        if (f === 'status') return this.statusOptionsForResource();
        if (f === 'currencycode' || f === 'currency' || f === 'preferredcurrency') {
            return ['USD', 'EUR', 'GBP', 'ZAR', 'BWP', 'ZMW', 'ZWG', 'ZWL', 'NAD', 'MZN', 'AOA', 'MUR', 'TZS', 'AUD', 'CAD', 'CHF', 'CNY', 'INR', 'JPY', 'SGD'];
        }
        return null;
    }

    private statusOptionsForResource(): string[] {
        switch (this.resource()) {
            case 'reservations': return ['Pending', 'Confirmed', 'Open', 'Completed', 'Closed', 'Cancelled', 'CheckedIn', 'CheckedOut', 'Refunded'];
            case 'invoices': return ['Pending', 'Paid', 'Cancelled', 'Void'];
            case 'payments': return ['Pending', 'Paid', 'Failed', 'Cancelled', 'Refunded'];
            case 'vouchers': return ['Active', 'Used', 'Expired', 'Cancelled'];
            case 'support-messages': return ['Open', 'In Progress', 'Resolved', 'Closed'];
            case 'commissions': return ['Pending', 'Approved', 'Paid', 'Cancelled'];
            case 'credit-notes': return ['Pending', 'Approved', 'Refunded', 'Cancelled'];
            case 'refunds': return ['Pending', 'Processed', 'Failed', 'Cancelled'];
            case 'proof-of-payments': return ['Pending', 'Verified', 'Rejected'];
            case 'day-end': return ['Open', 'Closed'];
            default: return ['Pending', 'Active', 'Inactive', 'Cancelled'];
        }
    }

    private cleanPayload(row: Record<string, any>): Record<string, any> {
        const payload = Object.fromEntries(
            Object.entries(row)
                .filter(([key, value]) => key !== 'serviceTypeCategory' && !this.isReadOnlyField(key) && value !== undefined)
                .map(([key, value]) => [key, value === '' ? null : value])
        );
        if (this.resource() === 'products') {
            payload['category'] = 'Product';
        }
        return payload;
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
                return ['serviceTypeCategory', 'name', 'description', 'basePriceUsd', 'isActive'];
            case 'accommodations':
                return ['type', 'description', 'capacity', 'guestCapacity', 'basePriceUsd', 'isActive'];
            case 'activities':
                return ['name', 'description', 'basePriceUsd', 'isActive'];
            case 'transfers':
            case 'tours':
                return ['name', 'description', 'isActive'];
            case 'service-prices':
                return ['serviceType', 'serviceId', 'currencyCode', 'unitPrice', 'isActive'];
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
            case 'credit-notes':
                return ['invoiceId', 'reservationId', 'reason', 'amount', 'currencyCode', 'status', 'notes'];
            case 'refunds':
                return ['paymentId', 'invoiceId', 'reason', 'amount', 'currencyCode', 'status'];
            case 'proof-of-payments':
                return ['paymentId', 'invoiceId', 'documentUrl', 'status', 'notes'];
            case 'commissions':
                return ['reservationId', 'consultantId', 'percentage', 'amount', 'currencyCode', 'status', 'notes'];
            case 'consultants':
                return ['firstName', 'lastName', 'email', 'phone', 'companyId', 'commissionRate', 'isActive'];
            case 'product-categories':
                return ['name', 'description', 'isActive'];
            case 'product-sub-categories':
                return ['categoryId', 'name', 'description', 'isActive'];
            case 'special-product-prices':
                return ['productId', 'productType', 'consultantId', 'specialPrice', 'currencyCode', 'startDate', 'endDate', 'notes', 'isActive'];
            default:
                return ['name', 'status'];
        }
    }

    private defaultValue(field: string): any {
        if (field === 'serviceTypeCategory') return this.serviceTypeForResource() ?? 'Product';
        if (field === 'preferredCurrency' || field === 'currency' || field === 'currencyCode') return 'USD';
        if (field === 'agentType') return 'Company';
        if (field === 'role') return 'regular';
        if (field === 'status') return this.resource() === 'support-messages' ? 'Open' : 'Pending';
        if (field === 'method') return 'Manual';
        if (field === 'type') return 'Reservation';
        if (field.toLowerCase().startsWith('is')) return true;
        if (field === 'guestCapacity') return 1;
        if (/amount|price|capacity|rate|percentage|id$/i.test(field)) return null;
        return '';
    }

    onDraftFieldChange(field: string): void {
        if (this.resource() === 'products' && field === 'serviceTypeCategory') {
            this.syncProductDraftFields();
        }
    }

    private loadUnifiedProducts(page = 1): void {
        this.loading.set(true);
        forkJoin({
            products: this.api.list<any>('/api/products'),
            accommodations: this.api.list<any>('/api/accommodations'),
            activities: this.api.list<any>('/api/activities'),
            transfers: this.api.list<any>('/api/transfers'),
            tours: this.api.list<any>('/api/tours')
        }).subscribe({
            next: (rows) => {
                const all = [
                    ...rows.products.map((item) => this.toUnifiedProductRow('Product', item)),
                    ...rows.accommodations.map((item) => this.toUnifiedProductRow('Accommodation', item)),
                    ...rows.activities.map((item) => this.toUnifiedProductRow('Activity', item)),
                    ...rows.transfers.map((item) => this.toUnifiedProductRow('Transfer', item)),
                    ...rows.tours.map((item) => this.toUnifiedProductRow('Tour', item))
                ].filter((item) => this.matchesUnifiedProductSearch(item));
                const start = (page - 1) * this.pageSize;
                this.rows.set(all.slice(start, start + this.pageSize));
                this.totalRecords.set(all.length);
                this.first.set(start);
                this.status.set('');
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(this.describeError(err));
                this.loading.set(false);
            }
        });
    }

    private toUnifiedProductRow(serviceType: string, item: any): Record<string, any> {
        const sourceId = Number(item.productId ?? item.id);
        return {
            sourceId,
            serviceTypeCategory: serviceType,
            name: item.name ?? item.type ?? serviceType,
            description: item.description ?? '',
            capacity: item.capacity ?? null,
            guestCapacity: item.guestCapacity ?? null,
            basePriceUsd: item.basePriceUsd ?? this.firstUsdPrice(item.prices),
            isActive: item.isActive !== false,
            createdDate: item.createdDate,
            sourceEndpoint: this.endpointForServiceType(serviceType)
        };
    }

    private firstUsdPrice(prices: unknown): number | null {
        if (!Array.isArray(prices)) return null;
        const usd = prices.find((price: any) => String(price.currencyCode ?? '').toUpperCase() === 'USD' && price.isActive !== false);
        return usd ? Number(usd.unitPrice ?? 0) : null;
    }

    private matchesUnifiedProductSearch(item: Record<string, any>): boolean {
        const q = this.query.trim().toLowerCase();
        if (!q) return true;
        return ['serviceTypeCategory', 'name', 'description']
            .some((field) => String(item[field] ?? '').toLowerCase().includes(q));
    }

    private saveUnifiedProduct(): void {
        const serviceType = String(this.draft['serviceTypeCategory'] ?? 'Product');
        const endpoint = this.endpointForServiceType(serviceType);
        const payload = this.payloadForUnifiedProduct(serviceType);
        const id = this.rowId(this.draft);

        this.saving.set(true);
        const request = this.dialogMode() === 'create'
            ? this.api.post<any>(endpoint, payload)
            : this.api.put<any>(`${endpoint}/${id}`, { ...payload, id, productId: id });

        request.subscribe({
            next: (saved) => {
                this.ensureUsdServicePrice(serviceType, saved, Number(this.draft['basePriceUsd'] ?? 0));
                this.saving.set(false);
                this.dialogVisible = false;
                this.messages.add({ severity: 'success', summary: 'Saved', detail: `${serviceType} saved under Manage Products.` });
                this.load(this.currentPage());
            },
            error: (err) => {
                this.saving.set(false);
                this.status.set(this.describeError(err));
            }
        });
    }

    private payloadForUnifiedProduct(serviceType: string): Record<string, any> {
        const name = String(this.draft['name'] ?? '').trim();
        const base = {
            name,
            description: this.draft['description'] ?? null,
            basePriceUsd: Number(this.draft['basePriceUsd'] ?? 0),
            isActive: this.draft['isActive'] !== false
        };

        if (serviceType === 'Accommodation') {
            return {
                type: name,
                description: base.description,
                capacity: Number(this.draft['capacity'] ?? 1),
                guestCapacity: Number(this.draft['guestCapacity'] ?? 1),
                basePriceUsd: base.basePriceUsd,
                isActive: base.isActive
            };
        }

        if (serviceType === 'Transfer' || serviceType === 'Tour') {
            return {
                name,
                description: base.description,
                isActive: base.isActive
            };
        }

        return serviceType === 'Product'
            ? { ...base, category: 'Product' }
            : base;
    }

    private ensureUsdServicePrice(serviceType: string, saved: any, unitPrice: number): void {
        if (!unitPrice || unitPrice <= 0 || !['Transfer', 'Tour'].includes(serviceType)) return;
        const serviceId = Number(saved?.id ?? saved?.productId);
        if (!serviceId) return;
        this.api.list<any>(`/api/service-prices/by-service/${serviceType}/${serviceId}`).subscribe({
            next: (prices) => {
                const usd = prices.find((price) => String(price.currencyCode ?? '').toUpperCase() === 'USD' && price.isActive !== false);
                const payload = { serviceType, serviceId, currencyCode: 'USD', unitPrice, isActive: true };
                const request = usd?.id
                    ? this.api.put(`/api/service-prices/${usd.id}`, { ...payload, id: usd.id })
                    : this.api.post('/api/service-prices', payload);
                request.subscribe({ error: () => undefined });
            },
            error: () => undefined
        });
    }

    private endpointForServiceType(serviceType: unknown): string {
        switch (String(serviceType)) {
            case 'Accommodation': return '/api/accommodations';
            case 'Activity': return '/api/activities';
            case 'Transfer': return '/api/transfers';
            case 'Tour': return '/api/tours';
            default: return '/api/products';
        }
    }

    private productFieldsForDraft(): string[] {
        const serviceType = String(this.draft['serviceTypeCategory'] ?? 'Product');
        const base = ['serviceTypeCategory', 'name', 'description', 'basePriceUsd', 'isActive'];
        return serviceType === 'Accommodation'
            ? ['serviceTypeCategory', 'name', 'description', 'capacity', 'guestCapacity', 'basePriceUsd', 'isActive']
            : base;
    }

    private syncProductDraftFields(): void {
        const serviceType = String(this.draft['serviceTypeCategory'] ?? 'Product');
        if (serviceType === 'Accommodation') {
            this.draft['capacity'] ??= 1;
            this.draft['guestCapacity'] ??= 1;
        } else {
            delete this.draft['capacity'];
            delete this.draft['guestCapacity'];
        }
        this.draft['basePriceUsd'] ??= null;
        this.draft['isActive'] ??= true;
    }

    private isSystemField(field: string): boolean {
        return this.isReadOnlyField(field) || (this.config()?.idFields ?? []).some((idField) => idField.toLowerCase() === field.toLowerCase());
    }

    private isReadOnlyField(field: string): boolean {
        return /created|updated|issued|processed/i.test(field) && field !== 'processedAt';
    }

    private serviceTypeForResource(): string | null {
        switch (this.resource()) {
            case 'accommodations': return 'Accommodation';
            case 'activities': return 'Activity';
            case 'transfers': return 'Transfer';
            case 'tours': return 'Tour';
            default: return null;
        }
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err, 'Entity request failed.');
    }
}
