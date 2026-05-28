import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { forkJoin } from 'rxjs';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService, SYSTEM_PAGE_SIZE } from '../../core/api/api.service';

interface OperationalFlowConfig {
    key: string;
    group: string;
    title: string;
    subtitle: string;
    endpoint?: string;
    icon: string;
    columns: string[];
    unsupportedActions?: string[];
    emptyMessage?: string;
}

const COMMON_CURRENCIES = [
    { code: 'USD', name: 'US Dollar' },
    { code: 'EUR', name: 'Euro' },
    { code: 'GBP', name: 'British Pound' },
    { code: 'JPY', name: 'Japanese Yen' },
    { code: 'AUD', name: 'Australian Dollar' },
    { code: 'CAD', name: 'Canadian Dollar' },
    { code: 'CHF', name: 'Swiss Franc' },
    { code: 'CNY', name: 'Chinese Yuan' },
    { code: 'HKD', name: 'Hong Kong Dollar' },
    { code: 'SGD', name: 'Singapore Dollar' },
    { code: 'SEK', name: 'Swedish Krona' },
    { code: 'KRW', name: 'South Korean Won' },
    { code: 'NOK', name: 'Norwegian Krone' },
    { code: 'NZD', name: 'New Zealand Dollar' },
    { code: 'INR', name: 'Indian Rupee' },
    { code: 'ZAR', name: 'South African Rand' },
    { code: 'BWP', name: 'Botswana Pula' },
    { code: 'ZMW', name: 'Zambian Kwacha' },
    { code: 'ZWL', name: 'Zimbabwe Dollar' },
    { code: 'ZWG', name: 'ZiG (Zimbabwe Gold)' },
    { code: 'MZN', name: 'Mozambican Metical' },
    { code: 'NAD', name: 'Namibian Dollar' },
    { code: 'AOA', name: 'Angolan Kwanza' },
    { code: 'MUR', name: 'Mauritian Rupee' },
    { code: 'TZS', name: 'Tanzanian Shilling' }
];

const FLOW_CONFIGS: Record<string, OperationalFlowConfig> = {
    'reservation-groups': {
        key: 'reservation-groups',
        group: 'Reservations',
        title: 'Trip Accounts',
        subtitle: 'Trip account view mapped to AppIt reservation records.',
        endpoint: '/api/reservations',
        icon: 'pi pi-users',
        columns: ['reference', 'customerEmail', 'currency', 'totalAmount', 'status']
    },
    'reservation-reports': {
        key: 'reservation-reports',
        group: 'Reservations',
        title: 'Reservation Reports',
        subtitle: 'Reservation reporting table using AppIt reservation data.',
        endpoint: '/api/reservations',
        icon: 'pi pi-file',
        columns: ['reference', 'voucherCode', 'customerEmail', 'totalAmount', 'status']
    },
    'availability-calendar': {
        key: 'availability-calendar',
        group: 'Reservations',
        title: 'Availability Calendar',
        subtitle: 'Calendar-style route represented as a 10-row operational table.',
        endpoint: '/api/reservations',
        icon: 'pi pi-calendar',
        columns: ['reference', 'customerEmail', 'currency', 'totalAmount', 'status']
    },
    'occupancy-calendar': {
        key: 'occupancy-calendar',
        group: 'Reservations',
        title: 'Occupancy Calendar',
        subtitle: 'Occupancy view backed by reservation volume and status.',
        endpoint: '/api/reservations',
        icon: 'pi pi-calendar',
        columns: ['reference', 'customerEmail', 'currency', 'totalAmount', 'status']
    },
    'flow-charts': {
        key: 'flow-charts',
        group: 'Reservations',
        title: 'Flow Charts',
        subtitle: 'Flow chart source rows from active AppIt reservations.',
        endpoint: '/api/reservations',
        icon: 'pi pi-chart-line',
        columns: ['reference', 'voucherCode', 'totalAmount', 'status']
    },
    'exchange-rates': {
        key: 'exchange-rates',
        group: 'Cashier',
        title: 'Manage Exchange Rates',
        subtitle: 'Set and manage currency exchange rates by date.',
        endpoint: '/api/exchange-rates',
        icon: 'pi pi-dollar',
        columns: ['currencyCode', 'rate', 'effectiveDate']
    },
    'bank-note-details': {
        key: 'bank-note-details',
        group: 'Cashier',
        title: 'Bank Note Details',
        subtitle: 'Cashier totals grouped from AppIt payment records.',
        endpoint: '/api/cashier/bank-note-details',
        icon: 'pi pi-wallet',
        columns: ['currency', 'method', 'status', 'quantity', 'total']
    },
    'cashier-reports': {
        key: 'cashier-reports',
        group: 'Cashier',
        title: 'Cashier Reports',
        subtitle: 'Cashier report view using AppIt payment rows.',
        endpoint: '/api/payments',
        icon: 'pi pi-file-export',
        columns: ['method', 'status', 'amount', 'currencyCode', 'processedAt']
    },
    invoicing: {
        key: 'invoicing',
        group: 'Accounts',
        title: 'Invoicing',
        subtitle: 'Manage invoices, generate statements, and close bookings.',
        endpoint: '/api/reservations',
        icon: 'pi pi-receipt',
        columns: ['voucherCode', 'reference', 'agent', 'consultant', 'customer', 'totalAmount', 'status', 'paymentStatus', 'amtDue', 'createdAt', 'closingByUserName']
    },
    'credit-notes': {
        key: 'credit-notes',
        group: 'Accounts',
        title: 'Manage Refunds',
        subtitle: 'Track and manage credit notes, refunds, and red-flagged transactions.',
        endpoint: '/api/reservations',
        icon: 'pi pi-undo',
        columns: ['voucherCode', 'invoiceId', 'agent', 'agencyConsultantId', 'customer', 'totalAmount', 'invoiced', 'redFlag', 'createdAt']
    },
    'deposit-reports': {
        key: 'deposit-reports',
        group: 'Accounts',
        title: 'Deposit Reports',
        subtitle: 'Deposit reporting view using AppIt payment records.',
        endpoint: '/api/payments',
        icon: 'pi pi-file-check',
        columns: ['transactionReference', 'method', 'status', 'amount', 'processedAt']
    },
    'proof-of-payments': {
        key: 'proof-of-payments',
        group: 'Accounts',
        title: 'Proof Of Payment',
        subtitle: 'Proof-of-payment review using AppIt payment records.',
        endpoint: '/api/payments',
        icon: 'pi pi-credit-card',
        columns: ['invoiceId', 'method', 'status', 'transactionReference', 'amount']
    },
    'check-in': {
        key: 'check-in',
        group: 'Operations',
        title: 'Check In',
        subtitle: 'Operations check-in queue from AppIt reservations and vouchers.',
        endpoint: '/api/reservations',
        icon: 'pi pi-check-circle',
        columns: ['reference', 'voucherCode', 'customerEmail', 'status', 'totalAmount'],
        unsupportedActions: ['Confirm Check In']
    },
    'opera-management': {
        key: 'opera-management',
        group: 'Operations',
        title: 'Opera Management',
        subtitle: 'Operations integration shell backed by AppIt vouchers.',
        endpoint: '/api/vouchers',
        icon: 'pi pi-sync',
        columns: ['code', 'reference', 'type', 'reservationId'],
        unsupportedActions: ['Sync Opera', 'Sync HConnect']
    },
    'user-activity': {
        key: 'user-activity',
        group: 'Administration',
        title: 'User Activity Log',
        subtitle: 'Activity log backed by AppIt audit logs.',
        endpoint: '/api/audit-logs',
        icon: 'pi pi-history',
        columns: ['entityName', 'action', 'performedBy', 'performedAt', 'changes']
    }
};

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, DialogModule, InputTextModule, SelectModule, TableModule, TagModule, TooltipModule],
    template: `
        <section class="grid gap-5">
            <div class="workspace-card flex flex-col xl:flex-row xl:items-end xl:justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">{{ config().group }}</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">{{ config().title }}</h1>
                    <p class="text-muted-color mt-2 mb-0">{{ config().subtitle }}</p>
            </div>
            </div>

            <article class="workspace-card" *ngIf="flowKey() === 'occupancy-calendar'">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div>
                        <h2 class="font-display text-2xl mt-0 mb-1">Monthly Occupancy Calendar</h2>
                        <p class="text-muted-color m-0">Confirmed and pending bookings by day with capacity usage.</p>
                    </div>
                    <div class="capacity-strip">
                        <span><strong>{{ confirmedCount() }}</strong> Confirmed</span>
                        <span><strong>{{ pendingCount() }}</strong> Pending</span>
                        <span><strong>{{ capacityPercent() }}%</strong> Capacity</span>
                    </div>
                </div>

                <div class="occupancy-calendar-grid">
                    <div class="calendar-weekday" *ngFor="let day of weekDays">{{ day }}</div>
                    <div class="calendar-day" *ngFor="let day of calendarDays()" [class.calendar-day-muted]="!day.inMonth">
                        <div class="calendar-day-head">
                            <strong>{{ day.date.getDate() }}</strong>
                            <span>{{ day.capacity }}%</span>
                        </div>
                        <div class="calendar-day-bars">
                            <span class="calendar-bar confirmed" [style.width.%]="day.confirmedPercent"></span>
                            <span class="calendar-bar pending" [style.width.%]="day.pendingPercent"></span>
                        </div>
                        <div class="calendar-day-meta">
                            <small>{{ day.confirmed }} confirmed</small>
                            <small>{{ day.pending }} pending</small>
                        </div>
                    </div>
                </div>
            </article>

            <article class="workspace-card" *ngIf="flowKey() === 'availability-calendar'">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div>
                        <h2 class="font-display text-2xl mt-0 mb-1">Accommodation Availability Calendar</h2>
                        <p class="text-muted-color m-0">Available units per accommodation type by day.</p>
                    </div>
                    <div class="capacity-strip">
                        <input pInputText type="month" [(ngModel)]="availMonth" (change)="loadAvailability()" />
                        <button pButton type="button" icon="pi pi-refresh" label="Refresh" (click)="loadAvailability()"></button>
                    </div>
                </div>

                <div class="occupancy-calendar-grid" *ngIf="availTypes().length">
                    <div class="calendar-weekday" *ngFor="let day of weekDays">{{ day }}</div>
                    <div class="avail-day" *ngFor="let day of availDays()" [class.calendar-day-muted]="!day.inMonth">
                        <div class="calendar-day-head">
                            <strong>{{ day.day }}</strong>
                        </div>
                        <div class="avail-type-list">
                            <div class="avail-type-row" *ngFor="let ta of day.typeAvailability">
                                <span class="avail-type-label">{{ ta.type }}:</span>
                                <span class="avail-type-value" [class.avail-low]="ta.available <= 0" [class.avail-ok]="ta.available > 0">{{ ta.available }}</span>
                            </div>
                        </div>
                    </div>
                </div>
                <p class="text-muted-color text-center py-4" *ngIf="!availTypes().length">No accommodation types configured.</p>
            </article>

            <article class="workspace-card" *ngIf="flowKey() === 'flow-charts'">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div>
                        <h2 class="font-display text-2xl mt-0 mb-1">Activity Flow Charts</h2>
                        <p class="text-muted-color m-0">Inline activity flow across confirmed, pending, completed, open, and cancelled bookings.</p>
                    </div>
                </div>
                <div class="flow-chart-inline">
                    <div class="flow-card" *ngFor="let flow of activityFlow()">
                        <div class="flow-card-head">
                            <strong>{{ flow.name }}</strong>
                            <span>{{ flow.total }} bookings</span>
                        </div>
                        <div class="flow-stack">
                            <span class="confirmed" [style.width.%]="flow.confirmedPercent"></span>
                            <span class="pending" [style.width.%]="flow.pendingPercent"></span>
                            <span class="completed" [style.width.%]="flow.completedPercent"></span>
                            <span class="cancelled" [style.width.%]="flow.cancelledPercent"></span>
                        </div>
                        <div class="flow-legend">
                            <small>Confirmed {{ flow.confirmed }}</small>
                            <small>Pending {{ flow.pending }}</small>
                            <small>Completed {{ flow.completed }}</small>
                            <small>Cancelled {{ flow.cancelled }}</small>
                        </div>
                    </div>
                </div>
            </article>

            <article class="workspace-card" *ngIf="flowKey() === 'exchange-rates'">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div class="flex items-center gap-3">
                        <i class="pi pi-dollar text-2xl text-primary"></i>
                        <div>
                            <h2 class="font-display text-2xl mt-0 mb-1">Manage Exchange Rates</h2>
                            <p class="text-muted-color m-0">Set exchange rates for each currency by date.</p>
                        </div>
                    </div>
                    <div class="flex items-center gap-2">
                        <label class="text-sm text-muted-color whitespace-nowrap">Date Effected</label>
                        <input pInputText type="date" [ngModel]="exchangeRateDate()" (ngModelChange)="onExchangeRateDateChange($event)" class="w-44" />
                        <button pButton type="button" icon="pi pi-plus" label="Add Rate" (click)="addExchangeRate()"></button>
                        <button pButton type="button" icon="pi pi-save" label="Save All" (click)="saveExchangeRates()" severity="success"></button>
                        <button pButton type="button" icon="pi pi-refresh" (click)="loadExchangeRates()" severity="secondary"></button>
                    </div>
                </div>

                <div class="mb-4">
                    <input pInputText [ngModel]="exchangeRateSearch()" (ngModelChange)="exchangeRateSearch.set($event)" placeholder="Search currencies..." class="w-full" />
                </div>

                <p-table [value]="filteredExchangeRates()" responsiveLayout="scroll" styleClass="p-datatable-sm">
                    <ng-template pTemplate="header">
                        <tr>
                            <th class="w-28">Currency</th>
                            <th class="w-40">Rate</th>
                            <th class="w-36">Date</th>
                            <th class="text-right">Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td><strong>{{ row.currencyCode }}</strong></td>
                            <td>
                                <input pInputText type="number" step="0.000001" [(ngModel)]="row.rate" class="w-36" />
                            </td>
                            <td>{{ row.effectiveDate ? (row.effectiveDate | date:'MM/dd/yyyy') : (exchangeRateDate() | date:'MM/dd/yyyy') }}</td>
                            <td class="text-right">
                                <div class="flex justify-end gap-2">
                                    <button pButton type="button" icon="pi pi-eye" rounded text class="app-row-action" aria-label="View rate" (click)="viewExchangeRate(row)"></button>
                                    <button pButton type="button" icon="pi pi-pencil" rounded text class="app-row-action" aria-label="Edit rate" (click)="editExchangeRate(row)"></button>
                                    <button pButton type="button" icon="pi pi-trash" rounded text class="app-row-action" severity="danger" aria-label="Delete rate" (click)="deleteExchangeRate(row)"></button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="4" class="text-center text-muted-color py-6">No rates set for this date. Click <strong>Add Rate</strong> to add one.</td>
                        </tr>
                    </ng-template>
                </p-table>
            </article>

            <article class="workspace-card" *ngIf="flowKey() === 'invoicing'">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div class="flex items-center gap-3">
                        <i class="pi pi-receipt text-2xl text-primary"></i>
                        <div>
                            <h2 class="font-display text-2xl mt-0 mb-1">Invoicing</h2>
                            <p class="text-muted-color m-0">View, filter, and manage reservation invoices.</p>
                        </div>
                    </div>
                    <div class="flex items-center gap-2 flex-wrap">
                        <select class="p-inputtext p-component w-32" [(ngModel)]="invoicingPeriod">
                            <option value="day">Day</option>
                            <option value="week">Week</option>
                            <option value="month">Month</option>
                            <option value="year">Year</option>
                        </select>
                        <select class="p-inputtext p-component w-40" [(ngModel)]="invoicingStatus">
                            <option value="All">All Statuses</option>
                            <option value="Confirmed">Confirmed</option>
                            <option value="Pending">Pending</option>
                            <option value="Completed">Completed</option>
                            <option value="Open">Open</option>
                            <option value="Cancelled">Cancelled</option>
                        </select>
                        <input pInputText type="text" [(ngModel)]="invoicingVoucher" placeholder="Voucher No..." class="w-40" />
                        <input pInputText type="date" [(ngModel)]="invoicingDate" class="w-40" />
                        <button pButton type="button" icon="pi pi-search" (click)="loadInvoicing()" severity="secondary"></button>
                        <button pButton type="button" icon="pi pi-refresh" (click)="clearInvoicingFilters()" severity="secondary"></button>
                    </div>
                </div>

                <p-table
                    [value]="invoicingRows()"
                    responsiveLayout="scroll"
                    styleClass="p-datatable-sm"
                    [loading]="loading()"
                    [rows]="10"
                    [paginator]="invoicingRows().length > 10"
                    [rowsPerPageOptions]="[10]"
                >
                    <ng-template pTemplate="header">
                        <tr>
                            <th>Voucher No</th>
                            <th>Voucher Ref</th>
                            <th>Agent</th>
                            <th>Consultant</th>
                            <th>Customer</th>
                            <th>Invoice Amount</th>
                            <th>Reservation Status</th>
                            <th>Payment Status</th>
                            <th>Amt Due</th>
                            <th>Date</th>
                            <th>Booked By</th>
                            <th class="text-right w-48">Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td>{{ row.voucherCode || '-' }}</td>
                            <td>{{ row.reference || '-' }}</td>
                            <td>{{ agentName(row) }}</td>
                            <td>{{ row.agencyConsultantId || '-' }}</td>
                            <td>{{ customerName(row) }}</td>
                            <td class="text-right">{{ display(row.totalAmount) }}</td>
                            <td><p-tag [value]="display(row.status)" [severity]="statusSeverity(row.status)"></p-tag></td>
                            <td><p-tag [value]="display(row.paymentStatus)" [severity]="paymentSeverity(row.paymentStatus)"></p-tag></td>
                            <td class="text-right font-semibold" [class.text-green-600]="amtDue(row) <= 0" [class.text-red-600]="amtDue(row) > 0">{{ amtDueDisplay(row) }}</td>
                            <td>{{ row.createdAt | date:'MM/dd/yyyy' }}</td>
                            <td>{{ row.closingByUserName || '-' }}</td>
                            <td class="text-right">
                                <div class="flex justify-end gap-1">
                                    <button pButton type="button" icon="pi pi-eye" rounded text class="app-row-action" pTooltip="View" (click)="view(row)"></button>
                                    <button pButton type="button" icon="pi pi-file-pdf" rounded text class="app-row-action" severity="success" pTooltip="Generate Statement" (click)="generateInvoiceStatement(row)"></button>
                                    <button pButton type="button" icon="pi pi-check-circle" rounded text class="app-row-action" severity="warn" pTooltip="Close Booking" (click)="closeBooking(row)"></button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="12" class="text-center text-muted-color py-6">No invoices found for the selected filters.</td>
                        </tr>
                    </ng-template>
                </p-table>
            </article>

            <article class="workspace-card" *ngIf="flowKey() === 'credit-notes'">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div class="flex items-center gap-3">
                        <i class="pi pi-undo text-2xl text-primary"></i>
                        <div>
                            <h2 class="font-display text-2xl mt-0 mb-1">Manage Refunds</h2>
                            <p class="text-muted-color m-0">Track and manage credit notes, refunds, and red-flagged transactions.</p>
                        </div>
                    </div>
                    <div class="flex items-center gap-2 flex-wrap">
                        <button pButton type="button" icon="pi pi-plus" label="Create Refund" (click)="cnCreateRefund()"></button>
                        <input pInputText type="text" [ngModel]="cnVoucher()" (ngModelChange)="cnVoucher.set($event)" placeholder="Search by voucher..." class="w-48" />
                        <button pButton type="button" icon="pi pi-search" (click)="loadCreditNotes()" severity="secondary"></button>
                    </div>
                </div>

                <div class="workspace-card p-3 mb-4 flex flex-wrap items-center gap-3 bg-surface-ground">
                    <button pButton type="button" [label]="cnFilterMode() === 'All' ? '• All' : 'All'" severity="secondary" [text]="cnFilterMode() !== 'All'" (click)="cnSetFilter('All')"></button>
                    <button pButton type="button" [label]="cnFilterMode() === 'Today' ? '• Today' : 'Today'" severity="secondary" [text]="cnFilterMode() !== 'Today'" (click)="cnSetFilter('Today')"></button>
                    <label class="flex items-center gap-1 text-sm">
                        Date:
                        <input pInputText type="date" [ngModel]="cnDate()" (ngModelChange)="cnDate.set($event); cnSetFilter('Date')" class="w-36" />
                    </label>
                    <label class="flex items-center gap-1 text-sm">
                        From:
                        <input pInputText type="date" [ngModel]="cnDateFrom()" (ngModelChange)="cnDateFrom.set($event); cnSetFilter('DateRange')" class="w-36" />
                    </label>
                    <label class="flex items-center gap-1 text-sm">
                        To:
                        <input pInputText type="date" [ngModel]="cnDateTo()" (ngModelChange)="cnDateTo.set($event); cnSetFilter('DateRange')" class="w-36" />
                    </label>
                    <label class="flex items-center gap-1 text-sm">
                        Month:
                        <input pInputText type="month" [ngModel]="cnMonth()" (ngModelChange)="cnMonth.set($event); cnSetFilter('Month')" class="w-36" />
                    </label>
                    <button pButton type="button" icon="pi pi-refresh" (click)="clearCnFilters()" severity="secondary" rounded text></button>
                </div>

                <p-table [value]="cnRows()" responsiveLayout="scroll" styleClass="p-datatable-sm" [loading]="loading()">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>Voucher No</th>
                            <th>Invoice No</th>
                            <th>Agent</th>
                            <th>Agent Consultant</th>
                            <th>Customer</th>
                            <th>Amount</th>
                            <th>Invoiced</th>
                            <th>Red Flag</th>
                            <th>Date</th>
                            <th class="text-right w-48">Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td>{{ row.voucherCode || '-' }}</td>
                            <td>{{ row.invoiceId || '-' }}</td>
                            <td>{{ agentName(row) }}</td>
                            <td>{{ row.agencyConsultantId || '-' }}</td>
                            <td>{{ customerName(row) }}</td>
                            <td class="text-right font-semibold" [class.text-red-600]="cnAmount(row) < 0">{{ cnAmountDisplay(row) }}</td>
                            <td>{{ cnInvoiced(row) }}</td>
                            <td>{{ cnRedFlag(row) }}</td>
                            <td>{{ row.createdAt | date:'MMM d, yyyy, h:mm:ss a' }}</td>
                            <td class="text-right">
                                <div class="flex justify-end gap-1">
                                    <button pButton type="button" icon="pi pi-eye" rounded text class="app-row-action" pTooltip="View" (click)="view(row)"></button>
                                    <button pButton type="button" icon="pi pi-undo" rounded text class="app-row-action" severity="warn" pTooltip="Issue Refund" (click)="issueRefund(row)"></button>
                                    <button pButton type="button" icon="pi pi-flag" rounded text class="app-row-action" severity="danger" pTooltip="Red Flag" (click)="toggleRedFlag(row)"></button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="10" class="text-center text-muted-color py-6">No credit notes found for the selected filters.</td>
                        </tr>
                    </ng-template>
                </p-table>
            </article>

            <p-dialog [header]="editDialogMode === 'add' ? 'Add Exchange Rate' : 'Edit Exchange Rate'" [(visible)]="editDialogVisible" [modal]="true" [style]="{ width: 'min(420px, 94vw)' }" [draggable]="false">
                <div class="grid gap-4" *ngIf="editRowData">
                    <label class="grid gap-1">
                        <span class="text-sm text-muted-color">Currency</span>
                        <select *ngIf="editDialogMode === 'add'" class="p-inputtext p-component" [(ngModel)]="editRowData.currencyCode">
                            <option value="" disabled>Select currency...</option>
                            <option *ngFor="let c of currencyOptions()" [value]="c.code">{{ c.code }} - {{ c.name }}</option>
                        </select>
                        <strong *ngIf="editDialogMode === 'edit'">{{ editRowData.currencyCode }}</strong>
                    </label>
                    <label class="grid gap-1">
                        <span class="text-sm text-muted-color">Rate</span>
                        <input pInputText type="number" step="0.000001" [(ngModel)]="editRowData.rate" />
                    </label>
                    <label class="grid gap-1">
                        <span class="text-sm text-muted-color">Effective Date</span>
                        <input pInputText type="date" [(ngModel)]="editRowData.effectiveDate" />
                    </label>
                </div>
                <div class="flex justify-end gap-2 pt-4">
                    <button pButton type="button" label="Cancel" severity="secondary" (click)="cancelEditDialog()"></button>
                    <button pButton type="button" label="Save" (click)="saveEditDialog()"></button>
                </div>
            </p-dialog>

            <article class="workspace-card" *ngIf="!isReservationOperationsView()">
                <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                    <div class="flex items-center gap-3">
                        <i [class]="config().icon + ' text-2xl text-primary'"></i>
                        <div>
                            <h2 class="font-display text-2xl mt-0 mb-1">{{ config().title }} Table</h2>
                            <p class="text-muted-color m-0">Rows are paginated at 10 across the system.</p>
                        </div>
                    </div>
                    <div class="flex flex-wrap gap-2" *ngIf="config().unsupportedActions?.length">
                        <button
                            pButton
                            type="button"
                            *ngFor="let action of config().unsupportedActions"
                            icon="pi pi-lock"
                            [label]="action"
                            severity="secondary"
                            outlined
                            (click)="showUnavailable(action)"
                        ></button>
                    </div>
                </div>

                <p-table
                    [value]="rows()"
                    [lazy]="!!config().endpoint"
                    [paginator]="true"
                    [rows]="pageSize"
                    [first]="first()"
                    [totalRecords]="totalRecords()"
                    [loading]="loading()"
                    [rowsPerPageOptions]="[10]"
                    responsiveLayout="scroll"
                    styleClass="p-datatable-sm"
                    (onLazyLoad)="onLazyLoad($event)"
                >
                    <ng-template pTemplate="header">
                        <tr>
                            <th *ngFor="let column of columns()">{{ label(column) }}</th>
                            <th class="text-right w-36">Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td *ngFor="let column of columns()">
                                <p-tag *ngIf="isStatusColumn(column)" [value]="display(row[column])" [severity]="statusSeverity(row[column])"></p-tag>
                                <span *ngIf="!isStatusColumn(column)">{{ display(row[column]) }}</span>
                            </td>
                            <td class="text-right">
                                <div class="flex justify-end gap-2">
                                    <button pButton type="button" icon="pi pi-eye" rounded text class="app-row-action" aria-label="View row" (click)="view(row)"></button>
                                    <button pButton type="button" icon="pi pi-file-pdf" rounded class="app-row-action app-pdf-button" aria-label="Export row PDF" (click)="exportRowPdf(row)"></button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td [attr.colspan]="columns().length + 1" class="text-center text-muted-color py-6">{{ emptyMessage() }}</td>
                        </tr>
                    </ng-template>
                </p-table>
            </article>

            <p-dialog
                header="Record Details"
                [(visible)]="detailVisible"
                [modal]="true"
                [style]="{ width: 'min(760px, 94vw)' }"
                [draggable]="false"
            >
                <dl class="grid grid-cols-1 md:grid-cols-2 gap-3 m-0" *ngIf="selectedRow">
                    <div class="border border-surface rounded-lg p-3" *ngFor="let item of detailEntries()">
                        <dt class="font-semibold text-muted-color">{{ label(item.key) }}</dt>
                        <dd class="m-0 mt-1 break-words">{{ display(item.value) }}</dd>
                    </div>
                </dl>
                <div class="flex justify-end pt-4">
                    <button pButton type="button" label="Close" severity="secondary" (click)="detailVisible = false"></button>
                </div>
            </p-dialog>

            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `,
    styles: [`
        .operation-filter-row,
        .capacity-strip,
        .flow-chart-inline,
        .flow-legend {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            align-items: center;
        }

        .operation-filter-row > * {
            min-width: 9.5rem;
        }

        .capacity-strip span {
            padding: 0.55rem 0.75rem;
            border: 1px solid var(--surface-border);
            border-radius: 0.5rem;
            background: var(--surface-ground);
        }

        .occupancy-calendar-grid {
            display: grid;
            grid-template-columns: repeat(7, minmax(0, 1fr));
            gap: 0.5rem;
        }

        .calendar-weekday {
            font-weight: 700;
            color: var(--text-color-secondary);
            text-align: center;
        }

        .calendar-day,
        .availability-row,
        .flow-card {
            border: 1px solid var(--surface-border);
            border-radius: 0.5rem;
            padding: 0.75rem;
            background: var(--surface-card);
            min-width: 0;
        }

        .calendar-day-muted {
            opacity: 0.45;
        }

        .calendar-day-head,
        .availability-row,
        .flow-card-head {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 0.75rem;
        }

        .calendar-day-bars,
        .availability-meter,
        .flow-stack {
            display: flex;
            height: 0.5rem;
            overflow: hidden;
            border-radius: 999px;
            background: var(--surface-ground);
            margin: 0.65rem 0;
        }

        .calendar-bar.confirmed,
        .flow-stack .confirmed {
            background: #22c55e;
        }

        .calendar-bar.pending,
        .flow-stack .pending {
            background: #f8b32d;
        }

        .flow-stack .completed {
            background: #3b82f6;
        }

        .flow-stack .cancelled {
            background: #ef4444;
        }

        .calendar-day-meta,
        .availability-row > div:first-child,
        .flow-card {
            display: grid;
            gap: 0.25rem;
        }

        .availability-list {
            display: grid;
            gap: 0.75rem;
        }

        .availability-meter {
            flex: 1 1 12rem;
            margin: 0;
        }

        .availability-meter span {
            background: #22c55e;
        }

        .flow-card {
            flex: 1 1 18rem;
        }

        .avail-day {
            border: 1px solid var(--surface-border);
            border-radius: 0.5rem;
            padding: 0.5rem;
            background: var(--surface-card);
            min-width: 0;
        }

        .avail-type-list {
            display: grid;
            gap: 0.15rem;
            margin-top: 0.35rem;
        }

        .avail-type-row {
            display: flex;
            justify-content: space-between;
            gap: 0.25rem;
            font-size: 0.7rem;
            line-height: 1.3;
        }

        .avail-type-label {
            color: var(--text-color-secondary);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .avail-type-value {
            font-weight: 700;
            white-space: nowrap;
        }

        .avail-type-value.avail-low {
            color: #ef4444;
        }

        .avail-type-value.avail-ok {
            color: #22c55e;
        }

        @media (max-width: 760px) {
            .occupancy-calendar-grid {
                grid-template-columns: repeat(2, minmax(0, 1fr));
            }
        }
    `]
})
export class OperationalFlowPage {
    private readonly api = inject(ApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly messages = inject(MessageService);

    readonly pageSize = SYSTEM_PAGE_SIZE;
    readonly rows = signal<any[]>([]);
    readonly totalRecords = signal(0);
    readonly first = signal(0);
    readonly loading = signal(false);
    readonly status = signal('');
    readonly flowKey = signal('reservation-reports');
    readonly allRows = signal<any[]>([]);
    readonly activities = signal<any[]>([]);
    selectedRow: Record<string, unknown> | null = null;
    detailVisible = false;
    query = '';
    period: 'day' | 'week' | 'month' | 'year' = 'month';
    statusFilter = 'All';
    agentFilter = 'All';
    activityFilter = 'All';
    dateFilter = '';
    dateFrom = '';
    dateTo = '';
    readonly weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    readonly availMonth = signal(this.defaultAvailMonth());
    readonly availData = signal<any[]>([]);
    readonly availTypes = signal<string[]>([]);
    readonly availDays = computed(() => this.availData());

    readonly exchangeRateDate = signal(this.toInputDate(new Date()));
    readonly exchangeRateSearch = signal('');
    readonly exchangeRateRows = signal<any[]>([]);
    readonly currenciesList = signal<any[]>([]);

    readonly invoicingPeriod = signal('month');
    readonly invoicingStatus = signal('All');
    readonly invoicingVoucher = signal('');
    readonly invoicingDate = signal(this.toInputDate(new Date()));
    readonly invoicingRows = signal<any[]>([]);

    readonly cnFilterMode = signal('All');
    readonly cnVoucher = signal('');
    readonly cnDate = signal('');
    readonly cnDateFrom = signal('');
    readonly cnDateTo = signal('');
    readonly cnMonth = signal('');
    readonly cnRows = signal<any[]>([]);
    readonly editDialogVisible = signal(false);
    editDialogMode: 'add' | 'edit' = 'edit';
    editRowData: any = null;

    readonly filteredExchangeRates = computed(() => {
        const rows = this.exchangeRateRows();
        const search = this.exchangeRateSearch().toLowerCase();
        if (!search) return rows;
        return rows.filter((r: any) => String(r.currencyCode).toLowerCase().includes(search));
    });

    readonly currencyOptions = computed(() => {
        const dbCodes = new Set(this.currenciesList().map((c: any) => c.code));
        const merged = [...this.currenciesList()];
        for (const c of COMMON_CURRENCIES) {
            if (!dbCodes.has(c.code)) {
                merged.push(c);
            }
        }
        return merged;
    });

    readonly periodOptions = [
        { label: 'Day', value: 'day' },
        { label: 'Week', value: 'week' },
        { label: 'Month', value: 'month' },
        { label: 'Year', value: 'year' }
    ];
    readonly statusOptions = [
        { label: 'All Statuses', value: 'All' },
        { label: 'Confirmed', value: 'Confirmed' },
        { label: 'Pending', value: 'Pending' },
        { label: 'Completed', value: 'Completed' },
        { label: 'Open', value: 'Open' },
        { label: 'Cancelled', value: 'Cancelled' }
    ];

    readonly config = computed(() => FLOW_CONFIGS[this.flowKey()] ?? FLOW_CONFIGS['reservation-reports']);
    readonly columns = computed(() => {
        const configured = this.config().columns;
        if (configured.length > 0) return configured;
        const row = this.rows()[0];
        return row ? Object.keys(row).filter((key) => typeof row[key] !== 'object').slice(0, 8) : [];
    });

    constructor() {
        this.route.data.subscribe((data) => {
            this.flowKey.set(data['flowKey'] ?? 'reservation-reports');
            this.query = '';
            this.load(1);
            if (data['flowKey'] === 'availability-calendar') {
                this.loadAvailability();
            } else if (data['flowKey'] === 'exchange-rates') {
                this.loadExchangeRates();
            } else if (data['flowKey'] === 'invoicing') {
                this.loadInvoicing();
            } else if (data['flowKey'] === 'credit-notes') {
                this.loadCreditNotes();
            } else {
                this.refreshAnalytics();
            }
        });
    }

    onLazyLoad(event: any): void {
        if (!this.config().endpoint) return;
        const page = Math.floor((event.first ?? 0) / this.pageSize) + 1;
        this.first.set(event.first ?? 0);
        this.load(page);
    }

    currentPage(): number {
        return Math.floor(this.first() / this.pageSize) + 1;
    }

    load(page = 1): void {
        const endpoint = this.config().endpoint;
        if (!endpoint) {
            this.rows.set([]);
            this.totalRecords.set(0);
            this.first.set(0);
            this.status.set('');
            return;
        }

        this.loading.set(true);
        this.api.listPage(endpoint, page, this.pageSize, this.query).subscribe({
            next: (result) => {
                this.rows.set(result.items ?? []);
                this.totalRecords.set(result.totalCount ?? result.items?.length ?? 0);
                this.first.set(((result.page ?? page) - 1) * this.pageSize);
                this.status.set('');
                this.loading.set(false);
            },
            error: (err) => {
                this.rows.set([]);
                this.totalRecords.set(0);
                this.status.set(this.describeError(err));
                this.loading.set(false);
            }
        });
    }

    refreshAnalytics(): void {
        if (!this.isReservationOperationsView()) {
            return;
        }

        this.api.listPage('/api/reservations', 1, 500, this.query).subscribe({
            next: (result) => {
                this.allRows.set(result.items ?? []);
                this.rows.set(this.filteredOperationalRows().slice(0, this.pageSize));
                this.totalRecords.set(this.filteredOperationalRows().length);
            },
            error: (err) => this.status.set(this.describeError(err))
        });

        this.api.list('/api/activities').subscribe({
            next: (rows) => this.activities.set(rows),
            error: () => this.activities.set([])
        });
    }

    clearAnalyticsFilters(): void {
        this.period = 'month';
        this.statusFilter = 'All';
        this.agentFilter = 'All';
        this.activityFilter = 'All';
        this.dateFilter = '';
        this.dateFrom = '';
        this.dateTo = '';
        this.refreshAnalytics();
    }

    loadAvailability(): void {
        const val = this.availMonth();
        if (!val) return;
        const [year, month] = val.split('-').map(Number);
        this.api.get(`/api/accommodations/availability?year=${year}&month=${month}`).subscribe({
            next: (data: any) => {
                this.availData.set(data?.days ?? []);
                this.availTypes.set(data?.types ?? []);
            },
            error: () => {
                this.availData.set([]);
                this.availTypes.set([]);
            }
        });
    }

    private defaultAvailMonth(): string {
        const now = new Date();
        return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    }

    private toInputDate(d: Date): string {
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    onExchangeRateDateChange(value: string): void {
        this.exchangeRateDate.set(value);
        this.loadExchangeRates();
    }

    loadExchangeRates(): void {
        const date = this.exchangeRateDate();
        if (!date) return;

        forkJoin({
            currencies: this.api.list('/api/currencies'),
            rates: this.api.get(`/api/exchange-rates/by-date?date=${date}`)
        }).subscribe({
            next: (result) => {
                this.currenciesList.set(result.currencies ?? []);
                this.mergeCurrenciesAndRates(Array.isArray(result.rates) ? result.rates : []);
            },
            error: () => {
                this.loading.set(false);
                this.messages.add({ severity: 'error', summary: 'Failed', detail: 'Could not load exchange rates.' });
            }
        });
    }

    private mergeCurrenciesAndRates(rates: any[]): void {
        const rateMap = new Map(rates.map((r: any) => [r.currencyCode, r]));
        const merged = this.currenciesList()
            .filter((c: any) => c.isActive !== false)
            .map((c: any) => {
                const existing = rateMap.get(c.code);
                return {
                    id: existing?.id ?? 0,
                    currencyCode: c.code,
                    rate: existing?.rate ?? 0,
                    effectiveDate: existing?.effectiveDate ?? this.exchangeRateDate()
                };
            });
        rates.forEach((r: any) => {
            if (!this.currenciesList().some((c: any) => c.code === r.currencyCode)) {
                merged.push({ ...r });
            }
        });
        this.exchangeRateRows.set(merged);
    }

    saveExchangeRates(): void {
        const date = this.exchangeRateDate();
        if (!date) return;

        const rows = this.exchangeRateRows().filter((r: any) => r.rate > 0);
        if (!rows.length) {
            this.messages.add({ severity: 'warn', summary: 'Nothing to save', detail: 'Set at least one rate above zero.' });
            return;
        }

        const obs = rows.map((r: any) => {
            if (r.id) {
                return this.api.put(`/api/exchange-rates/${r.id}`, { rate: r.rate, effectiveDate: date });
            }
            return this.api.post('/api/exchange-rates', { currencyCode: r.currencyCode, rate: r.rate, effectiveDate: date });
        });

        forkJoin(obs).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Saved', detail: 'Exchange rates saved successfully.' });
                this.loadExchangeRates();
            },
            error: () => {
                this.messages.add({ severity: 'error', summary: 'Failed', detail: 'Could not save exchange rates.' });
            }
        });
    }

    viewExchangeRate(row: any): void {
        this.selectedRow = row;
        this.detailVisible = true;
    }

    addExchangeRate(): void {
        this.editDialogMode = 'add';
        this.editRowData = {
            id: 0,
            currencyCode: '',
            rate: 0,
            effectiveDate: this.exchangeRateDate()
        };
        this.editDialogVisible.set(true);
    }

    editExchangeRate(row: any): void {
        this.editDialogMode = 'edit';
        this.editRowData = { ...row };
        this.editDialogVisible.set(true);
    }

    cancelEditDialog(): void {
        this.editDialogVisible.set(false);
        this.editRowData = null;
    }

    saveEditDialog(): void {
        if (!this.editRowData) return;
        const row = this.editRowData;
        const date = row.effectiveDate || this.exchangeRateDate();

        if (this.editDialogMode === 'edit' && row.id) {
            this.api.put(`/api/exchange-rates/${row.id}`, { rate: row.rate, effectiveDate: date }).subscribe({
                next: () => {
                    this.editDialogVisible.set(false);
                    this.editRowData = null;
                    this.messages.add({ severity: 'success', summary: 'Saved', detail: 'Exchange rate updated.' });
                    this.loadExchangeRates();
                },
                error: () => {
                    this.messages.add({ severity: 'error', summary: 'Failed', detail: 'Could not save exchange rate.' });
                }
            });
            return;
        }

        const doCreateRate = () => {
            this.api.post('/api/exchange-rates', { currencyCode: row.currencyCode, rate: row.rate, effectiveDate: date }).subscribe({
                next: () => {
                    this.editDialogVisible.set(false);
                    this.editRowData = null;
                    this.messages.add({ severity: 'success', summary: 'Added', detail: `Exchange rate for ${row.currencyCode} created.` });
                    this.loadExchangeRates();
                },
                error: () => {
                    this.messages.add({ severity: 'error', summary: 'Failed', detail: 'Could not save exchange rate.' });
                }
            });
        };

        const currencyExists = this.currenciesList().some((c: any) => c.code === row.currencyCode);
        if (currencyExists) {
            doCreateRate();
        } else {
            const name = COMMON_CURRENCIES.find((c) => c.code === row.currencyCode)?.name ?? row.currencyCode;
            this.api.post('/api/currencies', { name, code: row.currencyCode, isActive: true }).subscribe({
                next: () => doCreateRate(),
                error: () => this.messages.add({ severity: 'error', summary: 'Failed', detail: 'Could not create currency.' })
            });
        }
    }

    deleteExchangeRate(row: any): void {
        if (!row.id) {
            this.exchangeRateRows.update((rows) => rows.filter((r: any) => r !== row));
            return;
        }

        this.api.delete(`/api/exchange-rates/${row.id}`).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Deleted', detail: 'Exchange rate removed.' });
                this.loadExchangeRates();
            },
            error: () => {
                this.messages.add({ severity: 'error', summary: 'Failed', detail: 'Could not delete exchange rate.' });
            }
        });
    }

    loadInvoicing(): void {
        const page = 1;
        const searchParts: string[] = [];
        const status = this.invoicingStatus();
        if (status !== 'All') searchParts.push(`status=${status}`);
        const voucher = this.invoicingVoucher();
        if (voucher.trim()) searchParts.push(`voucherCode=${encodeURIComponent(voucher.trim())}`);
        const date = this.invoicingDate();
        if (date) searchParts.push(`date=${date}`);
        const period = this.invoicingPeriod();
        if (period !== 'month') searchParts.push(`period=${period}`);

        const query = searchParts.length ? `?${searchParts.join('&')}` : '';
        this.loading.set(true);
        this.api.listPage(`/api/reservations${query}`, page, 500, '').subscribe({
            next: (result) => {
                this.invoicingRows.set(result.items ?? []);
                this.loading.set(false);
            },
            error: () => {
                this.invoicingRows.set([]);
                this.loading.set(false);
            }
        });
    }

    clearInvoicingFilters(): void {
        this.invoicingPeriod.set('month');
        this.invoicingStatus.set('All');
        this.invoicingVoucher.set('');
        this.invoicingDate.set(this.toInputDate(new Date()));
        this.loadInvoicing();
    }

    agentName(row: any): string {
        const first = row.accountFirstName;
        const last = row.accountLastName;
        if (first || last) return [first, last].filter(Boolean).join(' ');
        return row.accountId ? `Agent #${row.accountId}` : '-';
    }

    customerName(row: any): string {
        const first = row.customerFirstName;
        const last = row.customerLastName;
        if (first || last) return [first, last].filter(Boolean).join(' ');
        return row.customerEmail || '-';
    }

    amtDue(row: any): number {
        const total = Number(row.totalAmount ?? 0);
        const paid = Number(row.paymentAmount ?? 0);
        return total - paid;
    }

    amtDueDisplay(row: any): string {
        const due = this.amtDue(row);
        const prefix = due > 0 ? '+' : '';
        return `${prefix}${due.toFixed(2)}`;
    }

    paymentSeverity(value: unknown): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
        const status = String(value ?? '').toLowerCase();
        if (status.includes('paid') || status.includes('complete')) return 'success';
        if (status.includes('partial') || status.includes('pending')) return 'warn';
        if (status.includes('fail') || status.includes('cancel') || status.includes('unpaid')) return 'danger';
        return 'info';
    }

    generateInvoiceStatement(row: any): void {
        const id = row.reservationId ?? row.id;
        if (!id) {
            this.messages.add({ severity: 'warn', summary: 'Cannot generate', detail: 'No reservation ID available.' });
            return;
        }
        this.api.postBlob('/api/exports/operational-row/pdf', {
            title: `Invoice Statement - ${row.voucherCode || row.reference || id}`,
            columns: ['voucherCode', 'reference', 'totalAmount', 'status', 'paymentStatus', 'createdAt'],
            row
        }).subscribe({
            next: (blob) => this.saveBlob(blob, `invoice-statement-${row.voucherCode || id}.pdf`),
            error: (err) => this.messages.add({
                severity: 'error', summary: 'Export failed', detail: this.describeError(err)
            })
        });
    }

    closeBooking(row: any): void {
        const id = row.reservationId ?? row.id;
        if (!id) {
            this.messages.add({ severity: 'warn', summary: 'Cannot close', detail: 'No reservation ID available.' });
            return;
        }
        this.api.put(`/api/reservations/${id}`, { status: 'Closed', closingByUserName: 'Admin' }).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Closed', detail: `Booking ${row.voucherCode || id} closed.` });
                this.loadInvoicing();
            },
            error: (err) => this.messages.add({
                severity: 'error', summary: 'Close failed', detail: this.describeError(err)
            })
        });
    }

    loadCreditNotes(): void {
        const searchParts: string[] = [];
        const mode = this.cnFilterMode();
        const voucher = this.cnVoucher();

        if (voucher.trim()) searchParts.push(`search=${encodeURIComponent(voucher.trim())}`);
        if (mode === 'Today') {
            const today = this.toInputDate(new Date());
            searchParts.push(`date=${today}`);
        } else if (mode === 'Date' && this.cnDate()) {
            searchParts.push(`date=${this.cnDate()}`);
        } else if (mode === 'DateRange' && this.cnDateFrom() && this.cnDateTo()) {
            searchParts.push(`dateFrom=${this.cnDateFrom()}&dateTo=${this.cnDateTo()}`);
        } else if (mode === 'Month' && this.cnMonth()) {
            searchParts.push(`month=${this.cnMonth()}`);
        }

        const query = searchParts.length ? `?${searchParts.join('&')}` : '';
        this.loading.set(true);
        this.api.listPage(`/api/reservations${query}`, 1, 500, '').subscribe({
            next: (result) => {
                this.cnRows.set(result.items ?? []);
                this.loading.set(false);
            },
            error: () => {
                this.cnRows.set([]);
                this.loading.set(false);
            }
        });
    }

    clearCnFilters(): void {
        this.cnFilterMode.set('All');
        this.cnVoucher.set('');
        this.cnDate.set('');
        this.cnDateFrom.set('');
        this.cnDateTo.set('');
        this.cnMonth.set('');
        this.loadCreditNotes();
    }

    cnSetFilter(mode: string): void {
        this.cnFilterMode.set(mode as any);
        this.loadCreditNotes();
    }

    cnCreateRefund(): void {
        this.messages.add({ severity: 'info', summary: 'Create Refund', detail: 'Refund creation dialog coming soon.' });
    }

    cnAmount(row: any): number {
        return -(Number(row.totalAmount ?? 0));
    }

    cnAmountDisplay(row: any): string {
        const neg = -(Number(row.totalAmount ?? 0));
        return `${neg.toFixed(2)}`;
    }

    cnInvoiced(row: any): string {
        return row.invoiceId ? 'Yes' : 'No';
    }

    cnRedFlag(row: any): string {
        const status = String(row.status ?? '').toLowerCase();
        return status === 'cancelled' || status === 'refunded' ? 'Yes' : 'No';
    }

    issueRefund(row: any): void {
        const id = row.reservationId ?? row.id;
        if (!id) {
            this.messages.add({ severity: 'warn', summary: 'Cannot refund', detail: 'No reservation ID available.' });
            return;
        }
        this.api.put(`/api/reservations/${id}`, { status: 'Refunded' }).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Refund issued', detail: `Refund for ${row.voucherCode || id} processed.` });
                this.loadCreditNotes();
            },
            error: (err) => this.messages.add({
                severity: 'error', summary: 'Refund failed', detail: this.describeError(err)
            })
        });
    }

    toggleRedFlag(row: any): void {
        const id = row.reservationId ?? row.id;
        if (!id) {
            this.messages.add({ severity: 'warn', summary: 'Cannot flag', detail: 'No reservation ID available.' });
            return;
        }
        const current = String(row.status ?? '');
        const newStatus = current === 'Cancelled' ? 'Pending' : 'Cancelled';
        this.api.put(`/api/reservations/${id}`, { status: newStatus }).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Updated', detail: `Reservation ${row.voucherCode || id} status changed to ${newStatus}.` });
                this.loadCreditNotes();
            },
            error: (err) => this.messages.add({
                severity: 'error', summary: 'Update failed', detail: this.describeError(err)
            })
        });
    }

    view(row: Record<string, unknown>): void {
        this.selectedRow = row;
        this.detailVisible = true;
    }

    exportPagePdf(): void {
        if (!this.rows().length) return;

        this.api.postBlob('/api/exports/operational-row/pdf', {
            title: `${this.config().title} Page Export`,
            columns: this.columns(),
            rows: this.rows()
        }).subscribe({
            next: (blob) => this.saveBlob(blob, `${this.slug(this.config().title)}-page.pdf`),
            error: (err) => {
                this.messages.add({
                    severity: 'error',
                    summary: 'PDF export failed',
                    detail: this.describeError(err)
                });
            }
        });
    }

    exportRowPdf(row: Record<string, unknown>, title = `${this.config().title} Row Export`): void {
        this.api.postBlob('/api/exports/operational-row/pdf', {
            title,
            columns: this.columns(),
            row
        }).subscribe({
            next: (blob) => this.saveBlob(blob, this.exportFileName(row, title)),
            error: (err) => {
                this.messages.add({
                    severity: 'error',
                    summary: 'PDF export failed',
                    detail: this.describeError(err)
                });
            }
        });
    }

    detailEntries(): Array<{ key: string; value: unknown }> {
        return Object.entries(this.selectedRow ?? {})
            .filter(([, value]) => typeof value !== 'object')
            .map(([key, value]) => ({ key, value }));
    }

    emptyMessage(): string {
        return this.config().emptyMessage ?? 'No rows loaded for this operational view.';
    }

    showUnavailable(action: string): void {
        this.messages.add({
            severity: 'info',
            summary: 'Frontend shell',
            detail: `${action} is visible for workflow parity and needs an AppIt backend endpoint before it can run.`
        });
    }

    isReservationOperationsView(): boolean {
        return ['occupancy-calendar', 'availability-calendar', 'flow-charts', 'exchange-rates', 'invoicing', 'credit-notes'].includes(this.flowKey());
    }

    confirmedCount(): number {
        return this.filteredOperationalRows().filter((row) => this.normalizedStatus(row).includes('confirm')).length;
    }

    pendingCount(): number {
        return this.filteredOperationalRows().filter((row) => this.normalizedStatus(row).includes('pending')).length;
    }

    capacityPercent(): number {
        return this.capacityFor(this.filteredOperationalRows().length);
    }

    totalFreeSlots(): number {
        return this.activityAvailability().reduce((sum, item) => sum + item.free, 0);
    }

    agentOptions(): Array<{ label: string; value: string }> {
        const agents = Array.from(new Set(this.allRows().map((row) => String(row.accountId ?? 'Unassigned'))));
        return [{ label: 'All Agents', value: 'All' }, ...agents.map((agent) => ({ label: `Agent ${agent}`, value: agent }))];
    }

    activityOptions(): Array<{ label: string; value: string }> {
        const names = new Set<string>();
        this.activities().forEach((item) => names.add(String(item.name ?? item.type ?? 'Activity')));
        this.allRows().flatMap((row) => this.servicesFor(row)).filter((item) => item.serviceType === 'Activity').forEach((item) => names.add(String(item.serviceName)));
        return [{ label: 'All Activities', value: 'All' }, ...Array.from(names).sort().map((name) => ({ label: name, value: name }))];
    }

    calendarDays(): Array<{ date: Date; inMonth: boolean; confirmed: number; pending: number; capacity: number; confirmedPercent: number; pendingPercent: number }> {
        const anchor = this.selectedDate();
        const first = new Date(anchor.getFullYear(), anchor.getMonth(), 1);
        const start = new Date(first);
        start.setDate(first.getDate() - first.getDay());
        return Array.from({ length: 42 }, (_, index) => {
            const date = new Date(start);
            date.setDate(start.getDate() + index);
            const rows = this.filteredOperationalRows().filter((row) => this.sameDay(this.rowDate(row), date));
            const confirmed = rows.filter((row) => this.normalizedStatus(row).includes('confirm')).length;
            const pending = rows.filter((row) => this.normalizedStatus(row).includes('pending')).length;
            const capacity = this.capacityFor(rows.length);
            return {
                date,
                inMonth: date.getMonth() === anchor.getMonth(),
                confirmed,
                pending,
                capacity,
                confirmedPercent: Math.min(100, confirmed * 10),
                pendingPercent: Math.min(100, pending * 10)
            };
        });
    }

    activityAvailability(): Array<{ name: string; booked: number; capacity: number; free: number; usedPercent: number }> {
        return this.activityOptions()
            .filter((item) => item.value !== 'All')
            .filter((item) => this.activityFilter === 'All' || item.value === this.activityFilter)
            .map((item) => {
                const booked = this.filteredOperationalRows().reduce((sum, row) => {
                    return sum + this.servicesFor(row).filter((service) => service.serviceType === 'Activity' && service.serviceName === item.value)
                        .reduce((serviceSum, service) => serviceSum + Number(service.quantity ?? 1), 0);
                }, 0);
                const capacity = 40;
                const free = Math.max(0, capacity - booked);
                return { name: item.value, booked, capacity, free, usedPercent: Math.min(100, (booked / capacity) * 100) };
            });
    }

    activityFlow(): Array<{ name: string; total: number; confirmed: number; pending: number; completed: number; cancelled: number; confirmedPercent: number; pendingPercent: number; completedPercent: number; cancelledPercent: number }> {
        return this.activityOptions()
            .filter((item) => item.value !== 'All')
            .filter((item) => this.activityFilter === 'All' || item.value === this.activityFilter)
            .map((item) => {
                const rows = this.filteredOperationalRows().filter((row) => this.servicesFor(row).some((service) => service.serviceType === 'Activity' && service.serviceName === item.value));
                const total = rows.length || 1;
                const confirmed = rows.filter((row) => this.normalizedStatus(row).includes('confirm')).length;
                const pending = rows.filter((row) => this.normalizedStatus(row).includes('pending') || this.normalizedStatus(row).includes('open')).length;
                const completed = rows.filter((row) => this.normalizedStatus(row).includes('complete') || this.normalizedStatus(row).includes('closed')).length;
                const cancelled = rows.filter((row) => this.normalizedStatus(row).includes('cancel')).length;
                return {
                    name: item.value,
                    total: rows.length,
                    confirmed,
                    pending,
                    completed,
                    cancelled,
                    confirmedPercent: (confirmed / total) * 100,
                    pendingPercent: (pending / total) * 100,
                    completedPercent: (completed / total) * 100,
                    cancelledPercent: (cancelled / total) * 100
                };
            });
    }

    isStatusColumn(column: string): boolean {
        return column.toLowerCase().includes('status');
    }

    statusSeverity(value: unknown): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
        const status = String(value ?? '').toLowerCase();
        if (status.includes('paid') || status.includes('active') || status.includes('complete')) return 'success';
        if (status.includes('pending') || status.includes('open')) return 'warn';
        if (status.includes('fail') || status.includes('cancel') || status.includes('deleted')) return 'danger';
        return 'info';
    }

    display(value: unknown): string {
        if (value === null || value === undefined || value === '') return '-';
        if (typeof value === 'boolean') return value ? 'Yes' : 'No';
        if (typeof value === 'number') return Number.isInteger(value) ? String(value) : value.toFixed(2);
        return String(value);
    }

    label(value: string): string {
        return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, (char) => char.toUpperCase());
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err, 'Operational request failed.');
    }

    private filteredOperationalRows(): any[] {
        return this.allRows().filter((row) => {
            const status = this.normalizedStatus(row);
            const date = this.rowDate(row);
            const services = this.servicesFor(row);
            const matchesStatus = this.statusFilter === 'All' || status.includes(this.statusFilter.toLowerCase());
            const matchesAgent = this.agentFilter === 'All' || String(row.accountId ?? 'Unassigned') === this.agentFilter;
            const matchesActivity = this.activityFilter === 'All' || services.some((item) => item.serviceType === 'Activity' && item.serviceName === this.activityFilter);
            const matchesDate = this.matchesDateWindow(date);
            return matchesStatus && matchesAgent && matchesActivity && matchesDate;
        });
    }

    private selectedDate(): Date {
        if (!this.dateFilter) return new Date();
        const filter = String(this.dateFilter);
        const date = new Date(`${filter}T00:00:00`);
        if (!isNaN(date.getTime())) return date;
        const parts = filter.split('-');
        if (parts.length === 2) return new Date(Number(parts[0]), Number(parts[1]) - 1);
        return new Date(Number(filter), 0);
    }

    private rowDate(row: any): Date {
        return new Date(row.createdAt ?? row.createdDate ?? row.issuedDate ?? Date.now());
    }

    private matchesDateWindow(date: Date): boolean {
        if (Number.isNaN(date.getTime())) return true;
        if (this.dateFrom && date < new Date(`${this.dateFrom}T00:00:00`)) return false;
        if (this.dateTo && date > new Date(`${this.dateTo}T23:59:59`)) return false;
        if (!this.dateFilter) return true;

        const selected = this.selectedDate();
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

    private normalizedStatus(row: any): string {
        return String(row.status ?? '').toLowerCase();
    }

    private servicesFor(row: any): any[] {
        return Array.isArray(row.serviceItems) ? row.serviceItems : [];
    }

    private capacityFor(bookings: number): number {
        return Math.min(100, Math.round((bookings / 40) * 100));
    }

    private saveBlob(blob: Blob, fileName: string): void {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(url);
    }

    private exportFileName(row: Record<string, unknown>, title: string): string {
        const identifier = row['reference'] ?? row['voucherCode'] ?? row['code'] ?? row['transactionReference'] ?? row['id'] ?? this.flowKey();
        return `${this.slug(title)}-${this.slug(String(identifier))}.pdf`;
    }

    private slug(value: string): string {
        return value
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/(^-|-$)/g, '') || 'export';
    }
}
