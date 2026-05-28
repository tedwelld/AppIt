import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { forkJoin } from 'rxjs';
import { ApiService, SYSTEM_PAGE_SIZE } from '../../core/api/api.service';
import { BookingCheckoutRequest, BookingCheckoutResult, BookingServiceItem, Customer } from '../../core/api/api.models';
import { AuthService } from '../../core/auth/auth.service';

interface ServiceOption {
    serviceType: 'Product' | 'Accommodation' | 'Activity';
    serviceId: number;
    serviceName: string;
    description: string;
    unitPrice: number;
    currency: string;
}

interface ServiceTypeOption {
    label: string;
    value: 'Product' | 'Accommodation' | 'Activity';
    endpoint: string;
}

interface PaymentMethodOption {
    label: string;
    value: string;
    provider: string;
}

interface TripAccountOption {
    tripAccountId: number;
    agentName: string;
    agentType: string;
    email?: string;
    phone?: string;
}

interface InvoiceForm {
    id: number | null;
    reservationId: number | null;
    totalAmount: number;
    currency: string;
    status: string;
    issuedAt?: string;
}

interface BookingForm {
    reservationId: number | null;
    reference: string;
    voucherCode: string;
    customerId: number | null;
    accountId: number | null;
    customerEmail: string;
    currency: string;
    totalAmount: number;
    status: string;
    closingByUserName: string;
}

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, DialogModule, SelectModule, InputTextModule, TableModule, TagModule],
    template: `
        <section class="grid gap-5">
            <div class="workspace-card flex flex-col xl:flex-row xl:items-center xl:justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">{{ isTripAccountsPage ? 'Trip Accounts' : 'Booking Capture' }}</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">{{ isTripAccountsPage ? 'Bookings & Trip Accounts' : 'Bookings' }}</h1>
                    <p class="text-muted-color mt-2 mb-0">Capture bookings, manage trip account rows, clone records into editable fields, and close completed bookings.</p>
                </div>
                <div class="flex flex-col sm:flex-row gap-2 w-full xl:w-auto">
                    <input pInputText class="w-full xl:w-80" [(ngModel)]="bookingSearch" (keyup.enter)="loadBookings(1)" placeholder="Search bookings..." />
                    <button pButton type="button" icon="pi pi-search" label="Search" severity="secondary" (click)="loadBookings(1)"></button>
                    <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="secondary" (click)="loadBookings(currentBookingPage())"></button>
                    <button pButton type="button" icon="pi pi-plus" label="New Booking" (click)="openBookingModal()"></button>
                </div>
            </div>

            <article class="workspace-card booking-table-card">
                <p-table
                    [value]="bookings()"
                    [lazy]="true"
                    [paginator]="true"
                    [rows]="pageSize"
                    [first]="bookingFirst()"
                    [totalRecords]="bookingTotalRecords()"
                    [loading]="bookingLoading()"
                    [rowsPerPageOptions]="[10]"
                    styleClass="p-datatable-sm booking-display-table"
                    [tableStyle]="{ 'min-width': '100%' }"
                    (onLazyLoad)="onBookingsLazyLoad($event)"
                >
                    <ng-template pTemplate="header">
                        <tr>
                            <th class="text-right w-20">Action</th>
                            <th>Voucher No</th>
                            <th>Voucher Ref</th>
                            <th>Agent</th>
                            <th>Customer</th>
                            <th>Status</th>
                            <th>Payment</th>
                            <th>Inv Amt</th>
                            <th>Due</th>
                            <th>Booked At</th>
                            <th>Closing</th>
                            <th>Closed By</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-booking>
                        <tr>
                            <td class="text-right whitespace-nowrap">
                                <button pButton type="button" icon="pi pi-eye" rounded text class="app-row-action" aria-label="View booking" (click)="viewBooking(booking)"></button>
                                <button
                                    pButton
                                    type="button"
                                    icon="pi pi-trash"
                                    rounded
                                    text
                                    severity="danger"
                                    class="app-row-action"
                                    aria-label="Delete booking"
                                    [title]="canDeleteBooking(booking) ? 'Delete booking' : 'Paid or invoiced-paid bookings cannot be hard deleted'"
                                    [disabled]="!canDeleteBooking(booking)"
                                    (click)="deleteBooking(booking)"
                                ></button>
                                <button
                                    pButton
                                    type="button"
                                    icon="pi pi-ban"
                                    rounded
                                    text
                                    severity="warn"
                                    class="app-row-action"
                                    aria-label="Cancel booking"
                                    title="Cancel booking"
                                    (click)="cancelBooking(booking)"
                                ></button>
                                <button pButton type="button" icon="pi pi-info-circle" rounded text severity="info" class="app-row-action" aria-label="Check status" (click)="checkStatus(booking)"></button>
                            </td>
                            <td>{{ booking.voucherCode ?? '-' }}</td>
                            <td>{{ booking.reference ?? '-' }}</td>
                            <td>{{ accountName(booking) }}</td>
                            <td>{{ customerName(booking) }}</td>
                            <td><p-tag [value]="booking.status ?? '-'" [severity]="statusSeverity(booking.status)"></p-tag></td>
                            <td><p-tag [value]="booking.paymentStatus ?? '-'" [severity]="statusSeverity(booking.paymentStatus)"></p-tag></td>
                            <td>{{ money($any(booking.invoiceTotalAmount) ?? 0, booking.invoiceCurrency ?? 'USD') }}</td>
                            <td>{{ amountDue(booking) }}</td>
                            <td>{{ display(booking.createdAt) }}</td>
                            <td>{{ display(booking.closingDate) }}</td>
                            <td>{{ booking.closingByUserName ?? '-' }}</td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr><td colspan="12" class="text-center text-muted-color py-4">No bookings found.</td></tr>
                    </ng-template>
                </p-table>
            </article>

            <p-dialog
                header="New Booking"
                [(visible)]="bookingModalVisible"
                [modal]="true"
                [style]="{ width: 'min(1500px, 99vw)' }"
                [contentStyle]="{ overflow: 'visible', padding: '0' }"
                [draggable]="false"
                styleClass="booking-modal"
            >
                <div class="booking-modal-shell">
                    <div class="booking-modal-intro">
                        <div>
                            <p class="text-primary font-bold uppercase tracking-[0.25em] m-0">Booking Wizard</p>
                            <h2 class="font-display text-3xl mt-2 mb-1">Capture Client Booking</h2>
                            <p class="text-muted-color m-0">Move through client details, services, and payment without leaving the bookings table.</p>
                        </div>
                        <div class="booking-total-chip">
                            <span>Total</span>
                            <strong>{{ money(total(), currency) }}</strong>
                        </div>
                    </div>

                    <div class="booking-step-strip">
                        <button pButton class="booking-step-button" type="button" *ngFor="let step of steps; let i = index" [label]="step" [severity]="activeStep() === i ? 'primary' : 'secondary'" (click)="activeStep.set(i)"></button>
                    </div>

                    <article class="booking-modal-panel" *ngIf="activeStep() === 0">
                        <h2 class="font-display text-2xl mt-0">Client Details</h2>
                        <div class="grid grid-cols-1 xl:grid-cols-[1.3fr_1fr] gap-5">
                            <div class="grid gap-3">
                                <div class="booking-customer-search">
                                    <input pInputText class="w-full" [(ngModel)]="customerSearch" placeholder="Search existing customers..." (keyup.enter)="searchCustomers()" />
                                    <button pButton type="button" class="booking-search-button" icon="pi pi-search" label="Search" (click)="searchCustomers()"></button>
                                </div>
                                <p-table [value]="customers()" styleClass="p-datatable-sm booking-display-table" [tableStyle]="{ 'min-width': '100%' }" [rows]="10" [paginator]="customers().length > 10">
                                    <ng-template pTemplate="header">
                                        <tr><th>Name</th><th>Contact</th><th class="text-right">Action</th></tr>
                                    </ng-template>
                                    <ng-template pTemplate="body" let-customer>
                                        <tr>
                                            <td>{{ customer.firstName || customer.proxyName || '-' }} {{ customer.surname || '' }}</td>
                                            <td>{{ customer.email || customer.phoneNumber || '-' }}</td>
                                            <td class="text-right"><button pButton type="button" icon="pi pi-check" rounded text class="app-row-action" aria-label="Select customer" (click)="selectCustomer(customer)"></button></td>
                                        </tr>
                                    </ng-template>
                                    <ng-template pTemplate="emptymessage">
                                        <tr><td colspan="3" class="text-center text-muted-color py-4">Search or enter a new client.</td></tr>
                                    </ng-template>
                                </p-table>
                            </div>

                            <form class="booking-client-form">
                                <label class="booking-inline-field booking-inline-field-wide">
                                    <span class="font-semibold">Trip Account Agent</span>
                                    <p-select
                                        [options]="tripAccounts()"
                                        [(ngModel)]="selectedTripAccountId"
                                        name="selectedTripAccountId"
                                        optionLabel="agentName"
                                        optionValue="tripAccountId"
                                        placeholder="Walk-in or company agent"
                                        [showClear]="true"
                                        [filter]="true"
                                        appendTo="body"
                                    ></p-select>
                                </label>
                                <label class="booking-inline-field">
                                    <span class="font-semibold">First Name</span>
                                    <input pInputText [(ngModel)]="client.firstName" name="firstName" />
                                </label>
                                <label class="booking-inline-field">
                                    <span class="font-semibold">Surname</span>
                                    <input pInputText [(ngModel)]="client.surname" name="surname" />
                                </label>
                                <label class="booking-inline-field">
                                    <span class="font-semibold">Email</span>
                                    <input pInputText type="email" [(ngModel)]="client.email" name="email" />
                                </label>
                                <label class="booking-inline-field">
                                    <span class="font-semibold">Phone</span>
                                    <input pInputText [(ngModel)]="client.phoneNumber" name="phoneNumber" />
                                </label>
                                <label class="booking-inline-field">
                                    <span class="font-semibold">Nationality</span>
                                    <input pInputText [(ngModel)]="client.nationality" name="nationality" />
                                </label>
                                <label class="booking-inline-field">
                                    <span class="font-semibold">Duration Of Stay</span>
                                    <input pInputText type="number" min="0" [(ngModel)]="client.durationOfStayDays" name="durationOfStayDays" />
                                </label>
                                <label class="booking-inline-field booking-inline-field-wide">
                                    <span class="font-semibold">Notes</span>
                                    <input pInputText [(ngModel)]="client.notes" name="notes" />
                                </label>
                            </form>
                        </div>
                    </article>

                    <article class="booking-modal-panel" *ngIf="activeStep() === 1">
                        <div class="flex flex-col xl:flex-row xl:items-end xl:justify-between gap-4">
                            <div>
                                <h2 class="font-display text-2xl mt-0">Services</h2>
                                <p class="text-muted-color m-0">Add products, accommodations, and activities to this booking.</p>
                            </div>
                            <div class="grid grid-cols-1 md:grid-cols-4 gap-2 w-full xl:w-auto">
                                <p-select [options]="serviceTypeOptions()" [(ngModel)]="selectedType" optionLabel="label" optionValue="value" (onChange)="selectedService = null" appendTo="body"></p-select>
                                <p-select [options]="filteredServices()" [(ngModel)]="selectedService" optionLabel="serviceName" placeholder="Select service" [filter]="true" appendTo="body"></p-select>
                                <input pInputText type="number" min="1" [(ngModel)]="selectedQuantity" placeholder="Qty" />
                                <button pButton type="button" icon="pi pi-plus" label="Add" (click)="addService()"></button>
                            </div>
                        </div>

                        <p-table [value]="serviceItems()" styleClass="p-datatable-sm mt-4 booking-display-table" [tableStyle]="{ 'min-width': '100%' }" [rows]="10" [paginator]="serviceItems().length > 10">
                            <ng-template pTemplate="header">
                                <tr><th>Type</th><th>Service</th><th>Qty</th><th>Unit</th><th>Total</th><th class="text-right">Actions</th></tr>
                            </ng-template>
                            <ng-template pTemplate="body" let-item let-index="rowIndex">
                                <tr>
                                    <td>{{ item.serviceType }}</td>
                                    <td>{{ item.serviceName }}</td>
                                    <td>{{ item.quantity }}</td>
                                    <td>{{ money(item.unitPrice, item.currency) }}</td>
                                    <td>{{ money(item.totalPrice || 0, item.currency) }}</td>
                                    <td class="text-right">
                                        <button pButton type="button" icon="pi pi-trash" severity="danger" rounded text class="app-row-action" aria-label="Remove service" (click)="removeService(index)"></button>
                                    </td>
                                </tr>
                            </ng-template>
                            <ng-template pTemplate="emptymessage">
                                <tr><td colspan="6" class="text-center text-muted-color py-4">No services selected.</td></tr>
                            </ng-template>
                        </p-table>
                    </article>

                    <article class="booking-modal-panel" *ngIf="activeStep() === 2">
                        <h2 class="font-display text-2xl mt-0">Payment And Review</h2>
                        <div class="grid grid-cols-1 xl:grid-cols-[1fr_320px] gap-5">
                            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <label class="grid gap-2">
                                    <span class="font-semibold">Payment Method</span>
                                    <p-select [options]="paymentMethods()" [(ngModel)]="paymentMethod" optionLabel="label" optionValue="value" placeholder="Select method" appendTo="body"></p-select>
                                </label>
                                <label class="grid gap-2">
                                    <span class="font-semibold">Currency</span>
                                    <input pInputText [(ngModel)]="currency" />
                                </label>
                                <label class="grid gap-2">
                                    <span class="font-semibold">Amount</span>
                                    <input pInputText type="number" [ngModel]="total()" name="amount" readonly />
                                </label>
                            </div>

                            <aside class="rounded-xl border border-surface-200 dark:border-surface-700 p-4">
                                <p class="text-muted-color text-sm m-0">Booking Total</p>
                                <p class="font-display text-4xl font-bold my-2">{{ money(total(), currency) }}</p>
                                <p class="m-0 text-sm">{{ serviceItems().length }} service rows selected</p>
                            </aside>
                        </div>
                    </article>

                    <div class="booking-modal-footer">
                        <p class="text-red-500 m-0" *ngIf="status()">{{ status() }}</p>
                        <div class="booking-modal-actions">
                            <button pButton type="button" label="Back" icon="pi pi-arrow-left" severity="secondary" (click)="back()" [disabled]="activeStep() === 0"></button>
                            <button pButton type="button" label="Next" icon="pi pi-arrow-right" iconPos="right" (click)="next()" *ngIf="activeStep() < 2"></button>
                            <button pButton type="button" label="Submit Booking" icon="pi pi-check" [loading]="submitting()" (click)="submit()" *ngIf="activeStep() === 2"></button>
                        </div>
                    </div>
                </div>
            </p-dialog>

            <p-dialog
                [header]="bookingActionHeader()"
                [(visible)]="bookingDetailVisible"
                [modal]="true"
                [style]="{ width: 'min(1500px, 99vw)' }"
                [contentStyle]="{ overflow: 'auto', 'max-height': '72vh' }"
                [draggable]="false"
                styleClass="booking-action-modal"
            >
                <div class="booking-action-tabs">
                    <button pButton type="button" label="Booking Details" icon="pi pi-calendar" [severity]="bookingDetailTab === 'booking' ? 'primary' : 'secondary'" (click)="bookingDetailTab = 'booking'"></button>
                    <button pButton type="button" label="Product Details" icon="pi pi-box" [severity]="bookingDetailTab === 'products' ? 'primary' : 'secondary'" (click)="bookingDetailTab = 'products'"></button>
                    <button pButton type="button" label="Payment Details" icon="pi pi-receipt" [severity]="bookingDetailTab === 'payment' ? 'primary' : 'secondary'" (click)="bookingDetailTab = 'payment'"></button>
                </div>

                <article class="booking-invoice-panel" *ngIf="selectedBooking && bookingDetailTab === 'booking'">
                    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                        <div>
                            <h3 class="font-display text-2xl mt-0 mb-1">Booking Details</h3>
                            <p class="text-muted-color m-0">Review the actual booking row first, then edit or clone it into the extension fields.</p>
                        </div>
                        <div class="booking-invoice-actions">
                            <button pButton type="button" icon="pi pi-pencil" label="Edit" severity="secondary" (click)="editBooking()" [disabled]="bookingActionSaving()"></button>
                            <button pButton type="button" icon="pi pi-copy" label="Clone" severity="secondary" (click)="cloneBooking()" [disabled]="bookingActionSaving()"></button>
                            <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="secondary" (click)="refreshBooking()" [disabled]="bookingActionSaving()"></button>
                        </div>
                    </div>

                    <div class="booking-edit-extension" *ngIf="bookingEditMode()">
                        <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-3">
                            <div>
                                <strong>Editable Booking Extension</strong>
                                <p class="text-muted-color m-0">Update the copied fields, save the changes, or close the booking from this same modal.</p>
                            </div>
                            <div class="booking-detail-invoice-actions">
                                <button pButton type="button" label="Save Update" icon="pi pi-save" (click)="saveBooking()" [loading]="bookingActionSaving()"></button>
                                <button pButton type="button" label="Close Booking" icon="pi pi-lock" severity="danger" (click)="closeBooking()" [disabled]="bookingActionSaving()"></button>
                            </div>
                        </div>
                        <form class="booking-invoice-form">
                            <label class="booking-inline-field">
                                <span class="font-semibold">Booking ID</span>
                                <input pInputText [ngModel]="bookingForm.reservationId ?? 'New'" name="bookingId" readonly />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Reference</span>
                                <input pInputText [(ngModel)]="bookingForm.reference" name="bookingReference" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Voucher Code</span>
                                <input pInputText [(ngModel)]="bookingForm.voucherCode" name="bookingVoucherCode" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Customer ID</span>
                                <input pInputText type="number" [(ngModel)]="bookingForm.customerId" name="bookingCustomerId" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Account ID</span>
                                <input pInputText type="number" [(ngModel)]="bookingForm.accountId" name="bookingAccountId" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Client Email</span>
                                <input pInputText type="email" [(ngModel)]="bookingForm.customerEmail" name="bookingCustomerEmail" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Currency</span>
                                <input pInputText [(ngModel)]="bookingForm.currency" name="bookingCurrency" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Total Amount</span>
                                <input pInputText type="number" min="0" step="0.01" [(ngModel)]="bookingForm.totalAmount" name="bookingTotalAmount" />
                            </label>
                            <label class="booking-inline-field">
                                <span class="font-semibold">Status</span>
                                <p-select [options]="bookingStatusOptions" [(ngModel)]="bookingForm.status" name="bookingStatus" optionLabel="label" optionValue="value" appendTo="body"></p-select>
                            </label>
                        </form>
                    </div>

                    <div class="booking-details-grid">
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Reservation Id</span>
                            <span class="booking-detail-value">{{ selectedBooking['reservationId'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Reference</span>
                            <span class="booking-detail-value">{{ selectedBooking['reference'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Voucher Code</span>
                            <span class="booking-detail-value">{{ selectedBooking['voucherCode'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Customer Id</span>
                            <span class="booking-detail-value">{{ selectedBooking['customerId'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Account Id</span>
                            <span class="booking-detail-value">{{ selectedBooking['accountId'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Currency</span>
                            <span class="booking-detail-value">{{ selectedBooking['currency'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Total Amount</span>
                            <span class="booking-detail-value">{{ money($any(selectedBooking['totalAmount']) ?? 0, $any(selectedBooking['currency']) ?? 'USD') }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Status</span>
                            <span class="booking-detail-value">{{ selectedBooking['status'] ?? '-' }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Created At</span>
                            <span class="booking-detail-value">{{ display(selectedBooking['createdAt']) }}</span>
                        </div>
                        <div class="booking-detail-item">
                            <span class="booking-detail-label">Customer Email</span>
                            <span class="booking-detail-value">{{ selectedBooking['customerEmail'] ?? '-' }}</span>
                        </div>
                    </div>

                    <p class="text-red-500 mt-3 mb-0" *ngIf="bookingActionStatus()">{{ bookingActionStatus() }}</p>
                </article>

                <article class="booking-invoice-panel" *ngIf="bookingDetailTab === 'products'">
                    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                        <div>
                            <h3 class="font-display text-2xl mt-0 mb-1">Product Details</h3>
                            <p class="text-muted-color m-0">Products, accommodations, and activities captured against this booking.</p>
                        </div>
                    </div>

                    <div class="booking-voucher-section" *ngIf="selectedVoucher() as voucher">
                        <div class="booking-voucher-header">
                            <i class="pi pi-ticket"></i>
                            <strong>Associated Voucher</strong>
                        </div>
                        <div class="booking-voucher-grid">
                            <div class="booking-detail-item">
                                <span class="booking-detail-label">Voucher Code</span>
                                <span class="booking-detail-value">{{ voucher['code'] ?? '-' }}</span>
                            </div>
                            <div class="booking-detail-item">
                                <span class="booking-detail-label">Type</span>
                                <span class="booking-detail-value">{{ voucher['type'] ?? '-' }}</span>
                            </div>
                            <div class="booking-detail-item">
                                <span class="booking-detail-label">Reference</span>
                                <span class="booking-detail-value">{{ voucher['reference'] ?? '-' }}</span>
                            </div>
                            <div class="booking-detail-item">
                                <span class="booking-detail-label">Combo Reference</span>
                                <span class="booking-detail-value">{{ voucher['comboReference'] ?? '-' }}</span>
                            </div>
                        </div>
                    </div>

                    <p-table [value]="selectedBookingServices()" styleClass="p-datatable-sm booking-display-table" [tableStyle]="{ 'min-width': '100%' }" [rows]="10" [paginator]="selectedBookingServices().length > 10">
                        <ng-template pTemplate="header">
                            <tr><th>Type</th><th>Service</th><th>Qty</th><th>Unit</th><th>Total</th></tr>
                        </ng-template>
                        <ng-template pTemplate="body" let-item>
                            <tr>
                                <td>{{ display(item.serviceType) }}</td>
                                <td>{{ display(item.serviceName) }}</td>
                                <td>{{ display(item.quantity) }}</td>
                                <td>{{ money(item.unitPrice || 0, item.currency || bookingForm.currency) }}</td>
                                <td>{{ money(item.totalPrice || 0, item.currency || bookingForm.currency) }}</td>
                            </tr>
                        </ng-template>
                        <ng-template pTemplate="emptymessage">
                            <tr><td colspan="5" class="text-center text-muted-color py-4">No product rows were captured for this booking.</td></tr>
                        </ng-template>
                    </p-table>
                </article>

                <article class="booking-invoice-panel" *ngIf="bookingDetailTab === 'payment'">
                    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                        <div>
                            <h3 class="font-display text-2xl mt-0 mb-1">Invoice Actions</h3>
                            <p class="text-muted-color m-0" *ngIf="invoiceForm.id">Editing invoice #{{ invoiceForm.id }} for reservation #{{ invoiceForm.reservationId }}.</p>
                            <p class="text-muted-color m-0" *ngIf="!invoiceForm.id">No invoice was found for this booking. Create one from the reservation details.</p>
                        </div>
                        <div class="booking-invoice-actions">
                            <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="secondary" (click)="refreshInvoice()" [disabled]="invoiceLoading()"></button>
                            <button pButton type="button" icon="pi pi-plus" label="New" severity="secondary" (click)="prepareNewInvoice()"></button>
                        </div>
                    </div>

                    <form class="booking-invoice-form">
                        <label class="booking-inline-field">
                            <span class="font-semibold">Invoice ID</span>
                            <input pInputText [ngModel]="invoiceForm.id ?? 'New'" name="invoiceId" readonly />
                        </label>
                        <label class="booking-inline-field">
                            <span class="font-semibold">Reservation ID</span>
                            <input pInputText type="number" [(ngModel)]="invoiceForm.reservationId" name="invoiceReservationId" />
                        </label>
                        <label class="booking-inline-field">
                            <span class="font-semibold">Total Amount</span>
                            <input pInputText type="number" min="0" step="0.01" [(ngModel)]="invoiceForm.totalAmount" name="invoiceTotalAmount" />
                        </label>
                        <label class="booking-inline-field">
                            <span class="font-semibold">Currency</span>
                            <input pInputText [(ngModel)]="invoiceForm.currency" name="invoiceCurrency" />
                        </label>
                        <label class="booking-inline-field">
                            <span class="font-semibold">Status</span>
                            <p-select [options]="invoiceStatusOptions" [(ngModel)]="invoiceForm.status" name="invoiceStatus" optionLabel="label" optionValue="value" appendTo="body"></p-select>
                        </label>
                        <label class="booking-inline-field">
                            <span class="font-semibold">Issued At</span>
                            <input pInputText [ngModel]="display(invoiceForm.issuedAt)" name="invoiceIssuedAt" readonly />
                        </label>
                    </form>

                    <p class="text-red-500 mt-3 mb-0" *ngIf="invoiceStatus()">{{ invoiceStatus() }}</p>
                </article>

                <ng-template pTemplate="footer">
                    <div class="booking-detail-footer">
                        <button pButton type="button" label="Close" severity="secondary" (click)="bookingDetailVisible = false"></button>
                        <div class="booking-detail-invoice-actions" *ngIf="bookingDetailTab === 'booking'">
                            <button pButton type="button" label="Edit" icon="pi pi-pencil" severity="secondary" (click)="editBooking()" [disabled]="bookingActionSaving()"></button>
                            <button pButton type="button" label="Clone" icon="pi pi-copy" severity="secondary" (click)="cloneBooking()" [disabled]="bookingActionSaving()"></button>
                            <button pButton type="button" label="Save Update" icon="pi pi-save" (click)="saveBooking()" [loading]="bookingActionSaving()" [disabled]="!bookingEditMode()"></button>
                            <button pButton type="button" label="Close Booking" icon="pi pi-lock" severity="danger" (click)="closeBooking()" [disabled]="!bookingEditMode() || bookingActionSaving()"></button>
                        </div>
                        <div class="booking-detail-invoice-actions" *ngIf="bookingDetailTab === 'payment'">
                            <button pButton type="button" label="Delete" icon="pi pi-trash" severity="danger" (click)="deleteInvoice()" [disabled]="!invoiceForm.id || invoiceSaving()"></button>
                            <button pButton type="button" label="Save Updates" icon="pi pi-save" (click)="saveInvoice()" [loading]="invoiceSaving()"></button>
                        </div>
                    </div>
                </ng-template>
            </p-dialog>

            <p-dialog header="Booking Created" [(visible)]="resultVisible" [modal]="true" [style]="{ width: 'min(720px, 94vw)' }" [draggable]="false">
                <div class="grid gap-3" *ngIf="result() as row">
                    <p><strong>Reservation:</strong> {{ row.reservation?.['reference'] || row.reservation?.['reservationId'] }}</p>
                    <p><strong>Invoice:</strong> {{ row.invoice?.['id'] }} - {{ money(row.invoice?.['totalAmount'] || 0, row.invoice?.['currency'] || currency) }}</p>
                    <p><strong>Payment:</strong> {{ row.payment?.['status'] }} via {{ row.payment?.['provider'] || paymentMethod }}</p>
                    <p><strong>Voucher:</strong> {{ row.voucher?.['code'] || '-' }}</p>
                </div>
                <ng-template pTemplate="footer">
                    <button pButton type="button" label="Close" (click)="resultVisible = false"></button>
                    <button pButton type="button" label="View Dashboard" severity="secondary" (click)="goDashboard()"></button>
                </ng-template>
            </p-dialog>
        </section>
    `,
    styles: [`
        .booking-table-card {
            width: 100%;
            padding: 0;
        }

        :host ::ng-deep .booking-table-card .p-datatable {
            width: 100%;
        }

        :host ::ng-deep .booking-display-table .p-datatable-table {
            width: 100%;
            min-width: 100%;
        }

        :host ::ng-deep .booking-display-table th,
        :host ::ng-deep .booking-display-table td {
            vertical-align: middle;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        :host ::ng-deep .booking-modal .p-dialog-header {
            padding: 1rem 1.25rem;
            border-bottom: 1px solid var(--surface-border);
        }

        :host ::ng-deep .booking-action-modal .p-dialog-header,
        :host ::ng-deep .booking-action-modal .p-dialog-footer {
            padding: 1rem 1.25rem;
        }

        :host ::ng-deep .booking-action-modal .p-dialog-header {
            border-bottom: 1px solid var(--surface-border);
        }

        :host ::ng-deep .booking-action-modal .p-dialog-footer {
            border-top: 1px solid var(--surface-border);
        }

        .booking-modal-shell {
            display: grid;
            gap: 1rem;
            padding: 1.25rem;
        }

        .booking-modal-intro {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            padding: 1rem;
            border: 1px solid var(--surface-border);
            border-radius: 0.75rem;
            background: color-mix(in srgb, var(--surface-card), var(--surface-ground) 28%);
        }

        .booking-total-chip {
            min-width: 11rem;
            display: grid;
            gap: 0.25rem;
            text-align: right;
        }

        .booking-total-chip span {
            color: var(--text-color-secondary);
            font-size: 0.85rem;
            font-weight: 700;
            text-transform: uppercase;
        }

        .booking-total-chip strong {
            font-size: 1.5rem;
        }

        .booking-step-strip {
            display: grid;
            grid-template-columns: repeat(3, minmax(0, 1fr));
            gap: 0.75rem;
        }

        :host ::ng-deep .booking-step-button.p-button,
        .booking-modal-actions .p-button {
            width: 100%;
            min-width: 10rem;
        }

        .booking-modal-panel {
            padding: 1rem;
            border: 1px solid var(--surface-border);
            border-radius: 0.75rem;
        }

        .booking-customer-search {
            display: grid;
            grid-template-columns: minmax(0, 1fr) 8.5rem;
            gap: 0.75rem;
            align-items: center;
        }

        :host ::ng-deep .booking-search-button.p-button {
            width: 8.5rem;
            min-width: 8.5rem;
        }

        .booking-client-form {
            display: grid;
            grid-template-columns: repeat(2, minmax(0, 1fr));
            gap: 0.6rem 1.25rem;
            align-content: start;
        }

        .booking-inline-field {
            display: grid;
            grid-template-columns: 9rem minmax(0, 1fr);
            gap: 0.75rem;
            align-items: center;
            min-width: 0;
        }

        .booking-inline-field-wide {
            grid-column: 1 / -1;
        }

        .booking-inline-field span {
            font-size: 0.9rem;
            line-height: 1.2;
            color: var(--text-color-secondary);
        }

        .booking-inline-field .p-inputtext {
            width: 100%;
        }

        .booking-inline-field p-select {
            width: 100%;
        }

        .booking-modal-footer {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            padding-top: 0.25rem;
        }

        .booking-modal-actions {
            display: grid;
            grid-auto-flow: column;
            grid-auto-columns: minmax(10rem, 1fr);
            gap: 0.75rem;
            margin-left: auto;
        }

        .booking-action-tabs,
        .booking-invoice-actions,
        .booking-detail-footer,
        .booking-detail-invoice-actions {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 0.75rem;
        }

        .booking-action-tabs {
            margin-bottom: 1rem;
            border-bottom: 1px solid var(--surface-border);
            padding-bottom: 0.75rem;
        }

        .booking-action-tabs .p-button {
            min-width: 10rem;
        }

        .booking-invoice-panel {
            padding: 1rem;
            border: 1px solid var(--surface-border);
            border-radius: 0.75rem;
        }

        .booking-edit-extension {
            display: grid;
            gap: 0.75rem;
            margin-bottom: 1rem;
            padding: 1rem;
            border: 1px solid var(--primary-color);
            border-radius: 0.5rem;
            background: color-mix(in srgb, var(--primary-color), transparent 92%);
        }

        .booking-invoice-form {
            display: grid;
            grid-template-columns: repeat(2, minmax(0, 1fr));
            gap: 0.75rem 1.5rem;
        }

        .booking-details-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(18rem, 1fr));
            gap: 1rem;
            padding: 0.5rem 0;
        }

        .booking-detail-item {
            display: flex;
            flex-direction: column;
            gap: 0.15rem;
        }

        .booking-detail-label {
            font-size: 0.75rem;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: var(--text-color-secondary);
        }

        .booking-detail-value {
            font-size: 0.95rem;
            font-weight: 600;
            color: var(--text-color);
            word-break: break-word;
        }

        .booking-voucher-section {
            margin-bottom: 1rem;
            padding: 0.85rem;
            border: 1px solid var(--primary-color);
            border-radius: 0.5rem;
            background: color-mix(in srgb, var(--primary-color), transparent 92%);
        }

        .booking-voucher-header {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 0.75rem;
            font-size: 0.9rem;
        }

        .booking-voucher-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr));
            gap: 0.75rem;
        }

        .booking-detail-footer {
            width: 100%;
            justify-content: space-between;
        }

        @media (max-width: 768px) {
            .booking-modal-intro,
            .booking-modal-footer {
                align-items: stretch;
                flex-direction: column;
            }

            .booking-total-chip {
                text-align: left;
            }

            .booking-step-strip,
            .booking-modal-actions {
                grid-template-columns: 1fr;
                grid-auto-flow: row;
            }

            .booking-customer-search,
            .booking-client-form,
            .booking-inline-field,
            .booking-invoice-form {
                grid-template-columns: 1fr;
            }

            .booking-detail-footer,
            .booking-detail-invoice-actions {
                align-items: stretch;
                flex-direction: column;
            }
        }
    `]
})
export class BookingWizardPage {
    private readonly api = inject(ApiService);
    private readonly auth = inject(AuthService);
    private readonly messages = inject(MessageService);
    private readonly router = inject(Router);
    private readonly route = inject(ActivatedRoute);

    readonly steps = ['Client', 'Services', 'Payment'];
    readonly activeStep = signal(0);
    readonly customers = signal<Customer[]>([]);
    readonly services = signal<ServiceOption[]>([]);
    readonly serviceItems = signal<BookingServiceItem[]>([]);
    readonly status = signal('');
    readonly submitting = signal(false);
    readonly result = signal<BookingCheckoutResult | null>(null);
    readonly bookings = signal<any[]>([]);
    readonly bookingTotalRecords = signal(0);
    readonly bookingFirst = signal(0);
    readonly bookingLoading = signal(false);
    readonly bookingActionSaving = signal(false);
    readonly bookingActionStatus = signal('');
    readonly invoiceLoading = signal(false);
    readonly invoiceSaving = signal(false);
    readonly invoiceStatus = signal('');
    readonly bookingEditMode = signal(false);
    readonly total = computed(() => this.serviceItems().reduce((sum, item) => sum + Number(item.totalPrice ?? 0), 0));
    readonly filteredServices = computed(() => this.services().filter((item) => item.serviceType === this.selectedType));
    readonly pageSize = SYSTEM_PAGE_SIZE;

    readonly serviceTypeOptions = signal<ServiceTypeOption[]>([]);
    readonly paymentMethods = signal<PaymentMethodOption[]>([]);
    readonly tripAccounts = signal<TripAccountOption[]>([]);

    customerSearch = '';
    selectedCustomer: Customer | null = null;
    client: Customer = { durationOfStayDays: 0 };
    selectedType: 'Product' | 'Accommodation' | 'Activity' = 'Product';
    selectedService: ServiceOption | null = null;
    selectedQuantity = 1;
    selectedTripAccountId: number | null = null;
    paymentMethod = 'Manual';
    currency = 'USD';
    resultVisible = false;
    bookingModalVisible = false;
    bookingDetailVisible = false;
    bookingSearch = '';
    isTripAccountsPage = false;
    selectedBooking: Record<string, unknown> | null = null;
    readonly selectedVoucher = signal<Record<string, unknown> | null>(null);
    bookingDetailTab: 'booking' | 'products' | 'payment' = 'booking';
    bookingForm: BookingForm = this.emptyBookingForm();
    invoiceForm: InvoiceForm = this.emptyInvoiceForm();
    readonly bookingStatusOptions = [
        { label: 'Pending', value: 'Pending' },
        { label: 'Confirmed', value: 'Confirmed' },
        { label: 'Closed', value: 'Closed' },
        { label: 'Cancelled', value: 'Cancelled' }
    ];
    readonly invoiceStatusOptions = [
        { label: 'Pending', value: 'Pending' },
        { label: 'Paid', value: 'Paid' },
        { label: 'Cancelled', value: 'Cancelled' },
        { label: 'Refunded', value: 'Refunded' }
    ];

    constructor() {
        this.isTripAccountsPage = !!this.route.snapshot.data['tripAccounts'];
        this.loadBookings(1);
        this.loadCatalog();
    }

    onBookingsLazyLoad(event: any): void {
        const page = Math.floor((event.first ?? 0) / this.pageSize) + 1;
        this.bookingFirst.set(event.first ?? 0);
        this.loadBookings(page);
    }

    loadBookings(page = 1): void {
        this.bookingLoading.set(true);
        this.api.listPage('/api/reservations', page, this.pageSize, this.bookingSearch).subscribe({
            next: (result) => {
                this.bookings.set(result.items ?? []);
                this.bookingTotalRecords.set(result.totalCount ?? result.items?.length ?? 0);
                this.bookingFirst.set(((result.page ?? page) - 1) * this.pageSize);
                this.bookingLoading.set(false);
            },
            error: (err) => {
                this.bookingLoading.set(false);
                this.status.set(this.describeError(err));
            }
        });
    }

    openBookingModal(): void {
        this.resetBookingForm();
        this.bookingModalVisible = true;
    }

    viewBooking(row: Record<string, unknown>): void {
        this.selectedBooking = row;
        this.selectedVoucher.set(null);
        this.bookingForm = this.toBookingForm(row);
        this.bookingActionStatus.set('');
        this.bookingEditMode.set(false);
        this.bookingDetailTab = 'booking';
        this.bookingDetailVisible = true;
        this.loadBookingDetails();
        this.loadBookingInvoice();
    }

    deleteBooking(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        if (!id) return;

        if (!this.canDeleteBooking(booking)) {
            this.messages.add({
                severity: 'warn',
                summary: 'Cannot delete booking',
                detail: 'This booking has paid invoice/payment records. Close or cancel the booking instead of hard deleting it.'
            });
            return;
        }

        if (!confirm('Are you sure you want to delete this booking?')) return;

        this.api.delete(`/api/reservations/${id}`).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Booking deleted', detail: 'The booking was removed.' });
                this.loadBookings(this.currentBookingPage());
            },
            error: (err) => this.messages.add({ severity: 'error', summary: 'Delete failed', detail: this.describeError(err) })
        });
    }

    canDeleteBooking(booking: Record<string, unknown>): boolean {
        const paymentStatus = String(booking['paymentStatus'] ?? '').toLowerCase();
        const invoiceStatus = String(booking['invoiceStatus'] ?? '').toLowerCase();
        return paymentStatus !== 'paid' && invoiceStatus !== 'paid';
    }

    cancelBooking(booking: Record<string, unknown>): void {
        const form = this.toBookingForm(booking);
        if (!form.reservationId) return;
        if (!confirm('Cancel this booking?')) return;

        this.api.put<any>(`/api/reservations/${form.reservationId}`, {
            reservationId: form.reservationId,
            reference: form.reference,
            voucherCode: form.voucherCode,
            customerId: this.optionalNumber(form.customerId),
            accountId: this.optionalNumber(form.accountId),
            agencyId: this.optionalNumber(Number(booking['agencyId'] ?? null)),
            currency: form.currency,
            totalAmount: Number(form.totalAmount),
            status: 'Cancelled',
            customerEmail: form.customerEmail || null
        }).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Booking cancelled', detail: 'The booking status was changed to Cancelled.' });
                this.loadBookings(this.currentBookingPage());
            },
            error: (err) => this.messages.add({ severity: 'error', summary: 'Cancel failed', detail: this.describeError(err) })
        });
    }

    checkStatus(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        const ref = booking['reference'] ?? booking['voucherCode'] ?? id;
        const status = booking['status'] ?? 'Unknown';
        this.messages.add({ severity: 'info', summary: `Booking ${ref}`, detail: `Current status: ${status}`, life: 4000 });
    }

    refreshBooking(): void {
        this.loadBookingDetails();
    }

    editBooking(): void {
        this.bookingForm = this.toBookingForm(this.selectedBooking);
        this.bookingEditMode.set(true);
        this.bookingActionStatus.set('');
    }

    saveBooking(closeAfterSave = false): void {
        const validation = this.validateBookingForm();
        if (validation) {
            this.bookingActionStatus.set(validation);
            return;
        }

        if (!this.bookingForm.reservationId) {
            this.bookingActionStatus.set('Booking ID is required before saving updates.');
            return;
        }

        this.bookingActionSaving.set(true);
        this.api.put<any>(`/api/reservations/${this.bookingForm.reservationId}`, {
            reservationId: this.bookingForm.reservationId,
            reference: this.bookingForm.reference,
            voucherCode: this.bookingForm.voucherCode,
            customerId: this.optionalNumber(this.bookingForm.customerId),
            accountId: this.optionalNumber(this.bookingForm.accountId),
            currency: this.bookingForm.currency,
            totalAmount: Number(this.bookingForm.totalAmount),
            status: this.bookingForm.status,
            customerEmail: this.bookingForm.customerEmail || null,
            closingByUserName: this.bookingForm.closingByUserName || null
        }).subscribe({
            next: (booking) => {
                this.bookingActionSaving.set(false);
                this.selectedBooking = booking;
                this.bookingForm = this.toBookingForm(booking);
                this.bookingEditMode.set(false);
                this.bookingActionStatus.set('');
                this.loadBookings(this.currentBookingPage());
                this.messages.add({ severity: 'success', summary: 'Booking saved', detail: 'Booking changes were saved.' });
                if (closeAfterSave) {
                    this.bookingDetailVisible = false;
                }
            },
            error: (err) => {
                this.bookingActionSaving.set(false);
                this.bookingActionStatus.set(this.describeError(err));
            }
        });
    }

    cloneBooking(): void {
        this.bookingForm = this.toBookingForm(this.selectedBooking);
        this.bookingEditMode.set(true);
        this.bookingActionStatus.set('');
        this.messages.add({ severity: 'info', summary: 'Booking copied', detail: 'The selected booking is now ready to edit.' });
    }

    closeBooking(): void {
        this.bookingForm.status = 'Closed';
        this.bookingForm.closingByUserName = this.auth.displayName();
        this.saveBooking(true);
    }

    selectedBookingServices(): BookingServiceItem[] {
        const services = this.selectedBooking?.['serviceItems'];
        return Array.isArray(services) ? services as BookingServiceItem[] : [];
    }

    bookingActionHeader(): string {
        const reference = this.bookingForm.reference || this.bookingForm.voucherCode || this.bookingForm.reservationId;
        return reference ? `Booking ${reference}` : 'Booking Actions';
    }

    refreshInvoice(): void {
        this.loadBookingInvoice();
    }

    prepareNewInvoice(): void {
        this.invoiceForm = this.emptyInvoiceForm();
        this.invoiceStatus.set('');
    }

    saveInvoice(): void {
        const validation = this.validateInvoiceForm();
        if (validation) {
            this.invoiceStatus.set(validation);
            return;
        }

        const payload = {
            id: this.invoiceForm.id,
            reservationId: Number(this.invoiceForm.reservationId),
            totalAmount: Number(this.invoiceForm.totalAmount),
            currency: this.invoiceForm.currency,
            status: this.invoiceForm.status
        };

        this.invoiceSaving.set(true);
        const request = this.invoiceForm.id
            ? this.api.put<any>(`/api/invoices/${this.invoiceForm.id}`, payload)
            : this.api.post<any>('/api/invoices', payload);

        request.subscribe({
            next: (invoice) => {
                this.invoiceSaving.set(false);
                this.invoiceForm = this.toInvoiceForm(invoice);
                this.invoiceStatus.set('');
                this.loadBookingDetails();
                this.loadBookings(this.currentBookingPage());
                this.messages.add({ severity: 'success', summary: 'Invoice saved', detail: 'Invoice changes were saved.' });
            },
            error: (err) => {
                this.invoiceSaving.set(false);
                this.invoiceStatus.set(this.describeError(err));
            }
        });
    }

    deleteInvoice(): void {
        if (!this.invoiceForm.id) return;

        this.invoiceSaving.set(true);
        this.api.delete(`/api/invoices/${this.invoiceForm.id}`).subscribe({
            next: () => {
                this.invoiceSaving.set(false);
                this.prepareNewInvoice();
                this.loadBookingDetails();
                this.loadBookings(this.currentBookingPage());
                this.messages.add({ severity: 'success', summary: 'Invoice deleted', detail: 'Invoice was removed from this booking.' });
            },
            error: (err) => {
                this.invoiceSaving.set(false);
                this.invoiceStatus.set(this.describeError(err));
            }
        });
    }

    searchCustomers(): void {
        this.api.searchCustomers(this.customerSearch).subscribe({
            next: (rows) => this.customers.set(rows),
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    selectCustomer(customer: Customer): void {
        this.selectedCustomer = customer;
        this.client = { ...customer };
        this.selectedTripAccountId = customer.agentCompanyId ?? this.selectedTripAccountId;
        this.messages.add({ severity: 'success', summary: 'Client Selected', detail: `${customer.firstName ?? 'Client'} ${customer.surname ?? ''}`.trim() });
    }

    addService(): void {
        if (!this.selectedService || this.selectedQuantity < 1) {
            this.status.set('Select a service and quantity.');
            return;
        }

        const quantity = Number(this.selectedQuantity);
        const item: BookingServiceItem = {
            serviceType: this.selectedService.serviceType,
            serviceId: this.selectedService.serviceId,
            serviceName: this.selectedService.serviceName,
            quantity,
            unitPrice: Number(this.selectedService.unitPrice),
            totalPrice: quantity * Number(this.selectedService.unitPrice),
            currency: this.selectedService.currency
        };
        this.currency = item.currency;
        this.serviceItems.update((rows) => [...rows, item]);
        this.selectedService = null;
        this.selectedQuantity = 1;
        this.status.set('');
    }

    removeService(index: number): void {
        this.serviceItems.update((rows) => rows.filter((_, rowIndex) => rowIndex !== index));
    }

    next(): void {
        if (!this.validateStep(this.activeStep())) return;
        this.activeStep.update((step) => Math.min(step + 1, 2));
    }

    back(): void {
        this.activeStep.update((step) => Math.max(step - 1, 0));
    }

    submit(): void {
        if (!this.validateStep(0) || !this.validateStep(1) || !this.validateStep(2)) return;
        const accountId = this.auth.user()?.id;
        const total = this.total();
        const request: BookingCheckoutRequest = {
            customerId: this.selectedCustomer?.id ?? null,
            tripAccountId: this.selectedTripAccountId,
            customer: this.selectedCustomer?.id ? null : { ...this.client, agentCompanyId: this.selectedTripAccountId },
            reservation: {
                accountId,
                agencyId: this.selectedTripAccountId,
                customerId: this.selectedCustomer?.id ?? null,
                currency: this.currency,
                totalAmount: total,
                status: 'Pending',
                customerEmail: this.client.email ?? null
            },
            invoice: {
                totalAmount: total,
                currency: this.currency,
                status: 'Pending'
            },
            payment: {
                method: this.paymentMethod,
                amount: total,
                currencyCode: this.currency,
                idempotencyKey: `booking-${Date.now()}-${Math.random().toString(16).slice(2)}`
            },
            serviceItems: this.serviceItems()
        };

        this.submitting.set(true);
        this.api.checkoutBooking(request).subscribe({
            next: (result) => {
                this.submitting.set(false);
                this.result.set(result);
                this.resultVisible = true;
                this.bookingModalVisible = false;
                this.loadBookings(1);
                this.messages.add({ severity: 'success', summary: 'Booking Created', detail: 'Reservation, invoice, payment, and voucher were captured.' });
            },
            error: (err) => {
                this.submitting.set(false);
                this.status.set(this.describeError(err));
            }
        });
    }

    goDashboard(): void {
        this.resultVisible = false;
        void this.router.navigateByUrl(this.auth.isAdmin() ? '/admin/dashboard' : '/user/dashboard');
    }

    money(value: number, currency = 'USD'): string {
        return new Intl.NumberFormat(undefined, { style: 'currency', currency: currency || 'USD' }).format(Number(value) || 0);
    }

    accountName(booking: Record<string, unknown>): string {
        const first = String(booking['accountFirstName'] ?? '');
        const last = String(booking['accountLastName'] ?? '');
        return (first || last) ? `${first} ${last}`.trim() : String(booking['accountId'] ?? '-');
    }

    customerName(booking: Record<string, unknown>): string {
        const first = String(booking['customerFirstName'] ?? '');
        const last = String(booking['customerLastName'] ?? '');
        const email = String(booking['customerEmail'] ?? '');
        return (first || last) ? `${first} ${last}`.trim() : (email || '-');
    }

    amountDue(booking: Record<string, unknown>): string {
        const services: any[] = Array.isArray(booking['serviceItems']) ? booking['serviceItems'] : [];
        const serviceTotal = services.reduce((sum, s) => sum + Number(s.totalPrice ?? 0), 0);
        const paid = Number(booking['paymentAmount'] ?? 0);
        const due = serviceTotal - paid;
        const prefix = due > 0 ? '+' : '';
        return `${prefix}${this.money(due, String(booking['currency'] ?? 'USD'))}`;
    }

    statusSeverity(value: unknown): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
        const status = String(value ?? '').toLowerCase();
        if (status === 'paid' || status === 'completed' || status === 'captured') return 'success';
        if (status === 'confirmed') return 'success';
        if (status === 'pending') return 'warn';
        if (status === 'closed') return 'info';
        if (status === 'cancelled') return 'danger';
        return 'secondary';
    }

    display(value: unknown): string {
        if (value === null || value === undefined || value === '') return '-';
        if (typeof value === 'boolean') return value ? 'Yes' : 'No';
        if (typeof value === 'object') return this.objectSummary(value);
        return String(value);
    }

    label(value: string): string {
        return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, (char) => char.toUpperCase());
    }

    private resetBookingForm(): void {
        this.activeStep.set(0);
        this.customers.set([]);
        this.serviceItems.set([]);
        this.status.set('');
        this.result.set(null);
        this.customerSearch = '';
        this.selectedCustomer = null;
        this.client = { durationOfStayDays: 0 };
        this.selectedType = this.serviceTypeOptions()[0]?.value ?? 'Product';
        this.selectedService = null;
        this.selectedQuantity = 1;
        this.selectedTripAccountId = null;
        this.paymentMethod = this.paymentMethods()[0]?.value ?? '';
        this.currency = 'USD';
    }

    private loadBookingDetails(): void {
        const reservationId = this.reservationIdFromSelectedBooking();
        if (!reservationId) return;

        this.api.get<any>(`/api/reservations/${reservationId}`).subscribe({
            next: (booking) => {
                this.selectedBooking = booking;
                this.bookingForm = this.toBookingForm(booking);
                this.bookingEditMode.set(false);
                this.loadBookingVoucher(reservationId);
            },
            error: (err) => this.bookingActionStatus.set(this.describeError(err))
        });
    }

    private loadBookingVoucher(reservationId: number): void {
        this.api.get<any>(`/api/vouchers/by-reservation/${reservationId}`).subscribe({
            next: (voucher) => this.selectedVoucher.set(voucher ?? null),
            error: () => this.selectedVoucher.set(null)
        });
    }

    private emptyBookingForm(): BookingForm {
        return {
            reservationId: null,
            reference: '',
            voucherCode: '',
            customerId: null,
            accountId: null,
            customerEmail: '',
            currency: 'USD',
            totalAmount: 0,
            status: 'Pending',
            closingByUserName: ''
        };
    }

    private toBookingForm(booking: Record<string, unknown> | null): BookingForm {
        return {
            reservationId: this.toNullableNumber(booking?.['reservationId'] ?? booking?.['id']),
            reference: String(booking?.['reference'] ?? ''),
            voucherCode: String(booking?.['voucherCode'] ?? ''),
            customerId: this.toNullableNumber(booking?.['customerId']),
            accountId: this.toNullableNumber(booking?.['accountId']),
            customerEmail: String(booking?.['customerEmail'] ?? ''),
            currency: String(booking?.['currency'] ?? 'USD'),
            totalAmount: Number(booking?.['totalAmount'] ?? 0),
            status: String(booking?.['status'] ?? 'Pending'),
            closingByUserName: String(booking?.['closingByUserName'] ?? '')
        };
    }

    private validateBookingForm(): string {
        if (!this.bookingForm.currency?.trim()) {
            return 'Booking currency is required.';
        }

        if (Number(this.bookingForm.totalAmount) < 0) {
            return 'Booking total cannot be negative.';
        }

        if (!this.bookingForm.status?.trim()) {
            return 'Booking status is required.';
        }

        return '';
    }

    private loadBookingInvoice(): void {
        const reservationId = this.reservationIdFromSelectedBooking();
        this.invoiceStatus.set('');
        this.invoiceForm = this.emptyInvoiceForm();

        if (!reservationId) {
            this.invoiceStatus.set('This booking does not have a reservation ID for invoice actions.');
            return;
        }

        this.invoiceLoading.set(true);
        this.api.get<any>(`/api/invoices/reservation/${reservationId}`).subscribe({
            next: (invoice) => {
                this.invoiceLoading.set(false);
                this.invoiceForm = invoice ? this.toInvoiceForm(invoice) : this.emptyInvoiceForm();
            },
            error: (err) => {
                this.invoiceLoading.set(false);
                if (err?.status === 404) {
                    this.invoiceForm = this.emptyInvoiceForm();
                    this.invoiceStatus.set('');
                    return;
                }

                this.invoiceStatus.set(this.describeError(err));
            }
        });
    }

    private emptyInvoiceForm(): InvoiceForm {
        const reservationId = this.reservationIdFromSelectedBooking();
        const totalAmount = Number(this.selectedBooking?.['totalAmount'] ?? 0);
        const currency = String(this.selectedBooking?.['currency'] ?? 'USD');
        return {
            id: null,
            reservationId,
            totalAmount,
            currency,
            status: 'Pending'
        };
    }

    private toInvoiceForm(invoice: any): InvoiceForm {
        return {
            id: Number(invoice.id),
            reservationId: Number(invoice.reservationId),
            totalAmount: Number(invoice.totalAmount ?? 0),
            currency: invoice.currency ?? 'USD',
            status: invoice.status ?? 'Pending',
            issuedAt: invoice.issuedAt
        };
    }

    private validateInvoiceForm(): string {
        if (!this.invoiceForm.reservationId || Number(this.invoiceForm.reservationId) <= 0) {
            return 'Reservation ID is required.';
        }

        if (Number(this.invoiceForm.totalAmount) < 0) {
            return 'Invoice total cannot be negative.';
        }

        if (!this.invoiceForm.currency?.trim()) {
            return 'Invoice currency is required.';
        }

        if (!this.invoiceForm.status?.trim()) {
            return 'Invoice status is required.';
        }

        return '';
    }

    private reservationIdFromSelectedBooking(): number | null {
        const value = this.selectedBooking?.['reservationId'] ?? this.selectedBooking?.['id'];
        const reservationId = Number(value);
        return Number.isFinite(reservationId) && reservationId > 0 ? reservationId : null;
    }

    currentBookingPage(): number {
        return Math.floor(this.bookingFirst() / this.pageSize) + 1;
    }

    private loadCatalog(): void {
        forkJoin({
            serviceTypes: this.api.list<ServiceTypeOption>('/api/frontend/service-types'),
            paymentMethods: this.api.list<PaymentMethodOption>('/api/frontend/payment-methods'),
            tripAccounts: this.api.list<TripAccountOption>('/api/trip-accounts'),
            products: this.api.list<any>('/api/products'),
            accommodations: this.api.list<any>('/api/accommodations'),
            activities: this.api.list<any>('/api/activities')
        }).subscribe({
            next: (rows) => {
                this.serviceTypeOptions.set(rows.serviceTypes);
                this.paymentMethods.set(rows.paymentMethods);
                this.tripAccounts.set(rows.tripAccounts);
                this.selectedType = rows.serviceTypes[0]?.value ?? 'Product';
                this.paymentMethod = rows.paymentMethods[0]?.value ?? '';
                this.services.set([
                    ...rows.products.map((item) => this.toServiceOption('Product', item)),
                    ...rows.accommodations.map((item) => this.toServiceOption('Accommodation', item)),
                    ...rows.activities.map((item) => this.toServiceOption('Activity', item))
                ]);
            },
            error: (err) => {
                this.services.set([]);
                this.serviceTypeOptions.set([]);
                this.paymentMethods.set([]);
                this.tripAccounts.set([]);
                this.status.set(this.describeError(err));
            }
        });
    }

    private toServiceOption(serviceType: 'Product' | 'Accommodation' | 'Activity', item: any): ServiceOption {
        return {
            serviceType,
            serviceId: Number(item.productId ?? item.id),
            serviceName: item.name ?? item.type ?? item.productName ?? serviceType,
            description: item.description ?? item.category ?? '',
            unitPrice: Number(item.basePriceUsd ?? item.price ?? 0),
            currency: 'USD'
        };
    }

    private validateStep(step: number): boolean {
        this.status.set('');
        if (step === 0) {
            const hasName = !!(this.client.firstName || this.client.surname);
            const hasContact = !!(this.client.email || this.client.phoneNumber);
            if (!hasName || !hasContact) {
                this.status.set('Client name and either email or phone are required.');
                return false;
            }
        }

        if (step === 1 && this.serviceItems().length === 0) {
            this.status.set('Add at least one service.');
            return false;
        }

        if (step === 2) {
            if (!this.paymentMethod) {
                this.status.set('Select a payment method.');
                return false;
            }
            if (this.total() <= 0) {
                this.status.set('Booking total must be greater than zero.');
                return false;
            }
        }

        return true;
    }

    private describeError(err: any): string {
        return this.errorMessage(err, 'Booking request failed.');
    }

    private errorMessage(value: unknown, fallback: string): string {
        if (!value) return fallback;
        if (typeof value === 'string') return value;
        if (value instanceof Blob) return fallback;
        if (typeof value !== 'object') return String(value);

        const record = value as Record<string, unknown>;
        const candidates = [
            record['message'],
            record['title'],
            record['detail'],
            (record['error'] as Record<string, unknown> | undefined)?.['message'],
            (record['error'] as Record<string, unknown> | undefined)?.['title'],
            (record['error'] as Record<string, unknown> | undefined)?.['detail'],
            record['error']
        ];

        for (const candidate of candidates) {
            if (!candidate) continue;
            if (typeof candidate === 'string') return candidate;
            if (typeof candidate === 'object') return this.objectSummary(candidate);
        }

        return fallback;
    }

    private objectSummary(value: unknown): string {
        if (!value || typeof value !== 'object') return String(value ?? '');
        const record = value as Record<string, unknown>;
        const errors = record['errors'];
        if (errors && typeof errors === 'object') {
            const messages = Object.values(errors as Record<string, unknown>)
                .flatMap((item) => Array.isArray(item) ? item : [item])
                .filter((item): item is string => typeof item === 'string');
            if (messages.length) return messages.join(' ');
        }

        const entries = Object.entries(record)
            .filter(([, item]) => item !== null && item !== undefined && typeof item !== 'object')
            .map(([key, item]) => `${this.label(key)}: ${String(item)}`);

        return entries.length ? entries.join(', ') : JSON.stringify(value);
    }

    private optionalNumber(value: number | null): number | null {
        return Number.isFinite(Number(value)) && Number(value) > 0 ? Number(value) : null;
    }

    private toNullableNumber(value: unknown): number | null {
        const numberValue = Number(value);
        return Number.isFinite(numberValue) && numberValue > 0 ? numberValue : null;
    }
}
