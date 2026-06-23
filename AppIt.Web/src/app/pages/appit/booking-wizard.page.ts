import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { forkJoin } from 'rxjs';
import { ApiService, SYSTEM_PAGE_SIZE } from '../../core/api/api.service';
import { BookingCheckoutRequest, BookingCheckoutResult, BookingServiceItem, Customer } from '../../core/api/api.models';
import { AuthService } from '../../core/auth/auth.service';

interface ServiceOption {
    serviceType: 'Product' | 'Accommodation' | 'Activity' | 'Transfer' | 'Tour' | 'Combo';
    serviceId: number;
    serviceName: string;
    description: string;
    categoryName: string;
    maxPax: number | null;
    unitPrice: number;
    productKind?: string;
    comboId?: number;
    // Stable USD base price; never mutated, so currency re-mapping never double-converts.
    basePriceUsd: number;
    currency: string;
    serviceLabel: string;
    hasSelectedCurrencyPrice: boolean;
    prices: Array<{ currencyCode: string; unitPrice: number; isActive?: boolean }>;
}

interface ServiceTypeOption {
    label: string;
    value: 'Product' | 'Accommodation' | 'Activity' | 'Transfer' | 'Tour' | 'Combo';
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

interface SupplierOption {
    supplierId: number;
    name: string;
    contactEmail?: string;
    contactPhone?: string;
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
    agencyConsultantId: number | null;
    agencyVoucherReference: string;
    country: string;
    paymentStatus: string;
    travelStatus: string;
    notes: string;
}

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, DialogModule, SelectModule, MultiSelectModule, InputTextModule, TableModule, TagModule, TooltipModule],
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
                    <select class="p-inputtext p-component w-44" [(ngModel)]="bookingStatusFilter" (ngModelChange)="loadBookings(1)">
                        <option value="">All Statuses</option>
                        <option value="Enquiry">Enquiry</option>
                        <option value="Provisional">Provisional</option>
                        <option value="Confirmed">Confirmed</option>
                        <option value="Closed">Closed</option>
                        <option value="Cancelled">Cancelled</option>
                    </select>
                    <button pButton type="button" icon="pi pi-search" label="Search" severity="info" (click)="loadBookings(1)"></button>
                    <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="warn" (click)="loadBookings(currentBookingPage())"></button>
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
                            <th class="text-right w-28">Actions</th>
                            <th>Voucher No</th>
                            <th>Voucher Ref</th>
                            <th>Agent</th>
                            <th>Customer</th>
                            <th>Status</th>
                            <th>Payment</th>
                            <th>Travel</th>
                            <th>Inv Amt</th>
                            <th>Due</th>
                            <th>Booked At</th>
                            <th>Closed By</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-booking>
                        <tr>
                            <td class="text-right whitespace-nowrap">
                                <button pButton type="button" icon="pi pi-eye" rounded text class="app-row-action" pTooltip="View" (click)="viewBooking(booking)"></button>
                                <button pButton type="button" icon="pi pi-lock" rounded text severity="danger" class="app-row-action" pTooltip="Close Booking" [disabled]="booking.status === 'Closed'" (click)="closeBookingFromTable(booking)"></button>
                                <button pButton type="button" icon="pi pi-ban" rounded text severity="warn" class="app-row-action" pTooltip="Cancel" [disabled]="booking.status === 'Cancelled'" (click)="cancelBookingFromTable(booking)"></button>
                                <button pButton type="button" icon="pi pi-refresh" rounded text severity="info" class="app-row-action" pTooltip="Re-open" [disabled]="booking.status !== 'Closed'" (click)="openBookingFromTable(booking)"></button>
                                <button pButton type="button" icon="pi pi-copy" rounded text severity="help" class="app-row-action" pTooltip="Clone" (click)="cloneBookingFromTable(booking)"></button>
                            </td>
                            <td>{{ booking.voucherCode ?? '-' }}</td>
                            <td>{{ booking.reference ?? '-' }}</td>
                            <td>{{ accountName(booking) }}</td>
                            <td>{{ customerName(booking) }}</td>
                            <td><p-tag [value]="booking.status ?? '-'" [severity]="statusSeverity(booking.status)"></p-tag></td>
                            <td><p-tag [value]="booking.paymentStatus ?? 'NotPaid'" [severity]="paymentStatusSeverity(booking.paymentStatus)"></p-tag></td>
                            <td><p-tag [value]="booking.travelStatus ?? 'NotCheckedIn'" [severity]="travelStatusSeverity(booking.travelStatus)"></p-tag></td>
                            <td>{{ money($any(booking.invoiceTotalAmount) ?? 0, booking.invoiceCurrency ?? 'USD') }}</td>
                            <td>{{ amountDue(booking) }}</td>
                            <td>{{ display(booking.createdAt) }}</td>
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
                [style]="{ width: 'min(1180px, 96vw)' }"
                [contentStyle]="{ overflow: 'auto', padding: '0', maxHeight: '88vh' }"
                [draggable]="false"
                styleClass="booking-modal"
            >
                <div class="booking-modal-shell">
                    <div class="booking-modal-intro">
                        <div>
                            <p class="text-primary font-bold uppercase tracking-[0.25em] m-0">SWA VCH N#{{ previewVoucherNumber() }}</p>
                            <h2 class="font-display text-2xl mt-2 mb-1">Capture Client Booking</h2>
                            <p class="text-muted-color m-0">Booking Details, Product Details, and Payment Details follow the same capture structure.</p>
                        </div>
                        <div class="booking-total-chip">
                            <span>Total</span>
                            <strong>{{ money(total(), currency) }}</strong>
                        </div>
                    </div>

                    <div class="booking-modal-body">
                        <nav class="booking-modal-steps" aria-label="Booking steps">
                            <button
                                pButton
                                type="button"
                                class="booking-step-nav"
                                *ngFor="let step of steps; let i = index"
                                [label]="step"
                                [icon]="i === 0 ? 'pi pi-user' : i === 1 ? 'pi pi-box' : 'pi pi-wallet'"
                                [severity]="activeStep() === i ? 'primary' : 'contrast'"
                                (click)="activeStep.set(i)"
                            ></button>
                        </nav>

                        <div class="booking-modal-main">
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
                                <label class="booking-stack-field booking-stack-field-wide">
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
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Agency Voucher Ref</span>
                                    <input pInputText [(ngModel)]="agencyVoucherRef" name="agencyVoucherRef" placeholder="Agent's own voucher number" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Consultant</span>
                                    <p-select
                                        [options]="consultants()"
                                        [(ngModel)]="selectedConsultantId"
                                        name="selectedConsultantId"
                                        optionLabel="fullName"
                                        optionValue="id"
                                        placeholder="Select consultant"
                                        [showClear]="true"
                                        [filter]="true"
                                        appendTo="body"
                                    ></p-select>
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">First Name</span>
                                    <input pInputText [(ngModel)]="client.firstName" name="firstName" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Surname</span>
                                    <input pInputText [(ngModel)]="client.surname" name="surname" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Email</span>
                                    <input pInputText type="email" [(ngModel)]="client.email" name="email" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Phone</span>
                                    <input pInputText [(ngModel)]="client.phoneNumber" name="phoneNumber" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Country</span>
                                    <input pInputText [(ngModel)]="clientCountry" name="country" placeholder="Country of origin" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Nationality</span>
                                    <input pInputText [(ngModel)]="client.nationality" name="nationality" />
                                </label>
                                <label class="booking-stack-field">
                                    <span class="font-semibold">Duration Of Stay</span>
                                    <input pInputText type="number" min="0" [(ngModel)]="client.durationOfStayDays" name="durationOfStayDays" />
                                </label>
                                <label class="booking-stack-field booking-stack-field-wide">
                                    <span class="font-semibold">Booking Notes</span>
                                    <input pInputText [(ngModel)]="bookingNotes" name="bookingNotes" placeholder="Special instructions or notes" />
                                </label>
                            </form>
                        </div>
                    </article>

                    <article class="booking-modal-panel" *ngIf="activeStep() === 1">
                        <div class="booking-services-header">
                            <div>
                                <h2 class="font-display text-2xl mt-0">Services</h2>
                                <p class="text-muted-color m-0">Choose catalog services grouped by the categories you created under Setup.</p>
                            </div>
                        </div>

                        <div class="booking-services-toolbar">
                            <label class="booking-stack-field">
                                <span class="font-semibold">Currency</span>
                                <p-select [options]="currencyOptions()" [(ngModel)]="currency" optionLabel="code" optionValue="code" (onChange)="onCurrencyChange(currency)" appendTo="body"></p-select>
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Service Type</span>
                                <p-select [options]="serviceTypeOptions()" [(ngModel)]="selectedType" optionLabel="label" optionValue="value" (onChange)="onServiceTypeChange()" appendTo="body"></p-select>
                            </label>
                            <label class="booking-stack-field booking-stack-field-span">
                                <span class="font-semibold">Service ({{ filteredServices().length }})</span>
                                <p-multiSelect
                                    [group]="true"
                                    [options]="groupedFilteredServices()"
                                    [(ngModel)]="selectedServices"
                                    optionLabel="serviceLabel"
                                    optionGroupLabel="label"
                                    optionGroupChildren="items"
                                    placeholder="Select one or more services"
                                    [filter]="true"
                                    display="chip"
                                    appendTo="body"
                                ></p-multiSelect>
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Supplier</span>
                                <p-select [options]="suppliers()" [(ngModel)]="lineSupplierId" optionLabel="name" optionValue="supplierId" placeholder="Optional" [showClear]="true" [filter]="true" appendTo="body"></p-select>
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Quantity</span>
                                <input pInputText type="number" min="1" [(ngModel)]="selectedQuantity" placeholder="Qty" />
                            </label>
                            <div class="booking-stack-field booking-services-add">
                                <span class="font-semibold">&nbsp;</span>
                                <button pButton type="button" icon="pi pi-plus" label="Add to booking" (click)="addService()"></button>
                            </div>
                        </div>

                        <div class="booking-line-details" *ngIf="selectedServices.length">
                                <label class="booking-stack-field"><span>Adults</span><input pInputText type="number" min="0" [(ngModel)]="lineAdultPax" placeholder="0" /></label>
                                <label class="booking-stack-field"><span>Children</span><input pInputText type="number" min="0" [(ngModel)]="lineChildPax" placeholder="0" /></label>
                                <label class="booking-stack-field"><span>Comps</span><input pInputText type="number" min="0" [(ngModel)]="lineCompPax" placeholder="0" /></label>
                                <label class="booking-stack-field" *ngIf="selectedType === 'Accommodation'"><span>Rooms</span><input pInputText type="number" min="1" [(ngModel)]="lineRooms" placeholder="1" /></label>
                                <label class="booking-stack-field" *ngIf="selectedType === 'Accommodation'"><span>Nights</span><input pInputText type="number" min="1" [(ngModel)]="lineNights" placeholder="1" /></label>
                                <label class="booking-stack-field" *ngIf="selectedType === 'Activity' || selectedType === 'Transfer' || selectedType === 'Tour'"><span>Pickup</span><input pInputText [(ngModel)]="linePickup" placeholder="Pickup location" /></label>
                                <label class="booking-stack-field" *ngIf="selectedType === 'Activity' || selectedType === 'Transfer' || selectedType === 'Tour'"><span>Dropoff</span><input pInputText [(ngModel)]="lineDropoff" placeholder="Dropoff location" /></label>
                                <label class="booking-stack-field" *ngIf="selectedType !== 'Product'"><span>Activity Date</span><input pInputText type="date" [(ngModel)]="lineActivityDate" /></label>
                                <label class="booking-stack-field"><span>Discount %</span><input pInputText type="number" min="0" max="100" step="0.01" [(ngModel)]="lineDiscountPercent" placeholder="0" /></label>
                        </div>

                        <p-table [value]="serviceItems()" styleClass="p-datatable-sm mt-4 booking-display-table" [tableStyle]="{ 'min-width': '100%' }" [rows]="10" [paginator]="serviceItems().length > 10">
                            <ng-template pTemplate="header">
                                <tr><th>Category</th><th>Service</th><th>Supplier</th><th>Qty</th><th>Adults</th><th>Children</th><th>Rooms</th><th>Nights</th><th>Disc%</th><th>Unit</th><th>Total</th><th class="text-right">Actions</th></tr>
                            </ng-template>
                            <ng-template pTemplate="body" let-item let-index="rowIndex">
                                <tr>
                                    <td>{{ serviceTypeLabel(item.serviceType) }}</td>
                                    <td>{{ item.serviceName }}</td>
                                    <td>{{ supplierName(item.supplierId) }}</td>
                                    <td>{{ item.quantity }}</td>
                                    <td>{{ item.adultPax ?? '-' }}</td>
                                    <td>{{ item.childPax ?? '-' }}</td>
                                    <td>{{ item.rooms ?? '-' }}</td>
                                    <td>{{ item.nights ?? '-' }}</td>
                                    <td>{{ item.discountPercent != null ? item.discountPercent + '%' : '-' }}</td>
                                    <td>{{ money(item.unitPrice, item.currency) }}</td>
                                    <td class="font-semibold">{{ money(effectiveTotal(item), item.currency) }}</td>
                                    <td class="text-right">
                                        <button pButton type="button" icon="pi pi-trash" severity="danger" rounded text class="app-row-action" aria-label="Remove service" (click)="removeService(index)"></button>
                                    </td>
                                </tr>
                            </ng-template>
                            <ng-template pTemplate="emptymessage">
                                <tr><td colspan="12" class="text-center text-muted-color py-4">No services selected.</td></tr>
                            </ng-template>
                        </p-table>
                        <div class="flex justify-end gap-6 pt-2 text-sm font-semibold" *ngIf="serviceItems().length > 0">
                            <span>Gross: {{ money(grossTotal(), currency) }}</span>
                            <span *ngIf="totalDiscount() > 0" class="text-green-600">Discount: -{{ money(totalDiscount(), currency) }}</span>
                            <span>Net: {{ money(netTotal(), currency) }}</span>
                        </div>
                    </article>

                    <article class="booking-modal-panel" *ngIf="activeStep() === 2">
                        <h2 class="font-display text-2xl mt-0">Payment And Review</h2>
                        <div class="grid grid-cols-1 xl:grid-cols-[1fr_320px] gap-5">
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <label class="grid gap-2">
                                    <span class="font-semibold">Payment Method</span>
                                    <p-select [options]="paymentMethods()" [(ngModel)]="paymentMethod" optionLabel="label" optionValue="value" placeholder="Select method" appendTo="body"></p-select>
                                </label>
                                <label class="grid gap-2">
                                    <span class="font-semibold">Currency</span>
                                    <p-select [options]="currencyOptions()" [(ngModel)]="currency" optionLabel="code" optionValue="code" (onChange)="onCurrencyChange(currency)" appendTo="body"></p-select>
                                </label>
                                <label class="grid gap-2">
                                    <span class="font-semibold">Bank / Transaction Reference</span>
                                    <input pInputText [(ngModel)]="transactionReference" placeholder="Bank reference or receipt number" />
                                </label>
                                <label class="grid gap-2">
                                    <span class="font-semibold">Proof Of Payment URL</span>
                                    <input pInputText [(ngModel)]="proofOfPaymentUrl" placeholder="https://..." />
                                </label>
                                <label class="grid gap-2 md:col-span-2">
                                    <span class="font-semibold">Payment Status</span>
                                    <select class="p-inputtext p-component" [(ngModel)]="bookingPaymentStatus">
                                        <option value="NotPaid">Not Paid</option>
                                        <option value="Deposited">Deposited</option>
                                        <option value="FullyPaid">Fully Paid</option>
                                    </select>
                                </label>
                            </div>

                            <aside class="rounded-xl border border-surface-200 dark:border-surface-700 p-4 grid gap-3">
                                <div>
                                    <p class="text-muted-color text-sm m-0">Gross Total</p>
                                    <p class="font-display text-3xl font-bold my-1">{{ money(grossTotal(), currency) }}</p>
                                </div>
                                <div *ngIf="totalDiscount() > 0">
                                    <p class="text-muted-color text-sm m-0">Discount</p>
                                    <p class="font-semibold text-green-600 my-1">-{{ money(totalDiscount(), currency) }}</p>
                                </div>
                                <div>
                                    <p class="text-muted-color text-sm m-0">Net Total</p>
                                    <p class="font-display text-xl font-bold my-1">{{ money(netTotal(), currency) }}</p>
                                </div>
                                <p class="m-0 text-sm text-muted-color">{{ serviceItems().length }} service rows</p>
                                <p-tag [value]="bookingPaymentStatus" [severity]="paymentStatusSeverity(bookingPaymentStatus)"></p-tag>
                            </aside>
                        </div>
                    </article>

                    <div class="booking-modal-footer">
                        <p class="text-red-500 m-0" *ngIf="status()">{{ status() }}</p>
                        <div class="booking-modal-actions">
                            <button pButton type="button" label="Back" icon="pi pi-arrow-left" severity="contrast" (click)="back()" [disabled]="activeStep() === 0"></button>
                            <button pButton type="button" label="Next" icon="pi pi-arrow-right" iconPos="right" (click)="next()" *ngIf="activeStep() < 2"></button>
                            <button pButton type="button" label="Save Enquiry" icon="pi pi-pencil" severity="info" [loading]="submitting()" (click)="submitWithStatus('Enquiry')" *ngIf="activeStep() === 2"></button>
                            <button pButton type="button" label="Provisional" icon="pi pi-clock" severity="warn" [loading]="submitting()" (click)="submitWithStatus('Provisional')" *ngIf="activeStep() === 2"></button>
                            <button pButton type="button" label="Confirm Booking" icon="pi pi-check" [loading]="submitting()" (click)="submitWithStatus('Confirmed')" *ngIf="activeStep() === 2"></button>
                        </div>
                    </div>
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
                    <button pButton type="button" label="Booking Details" icon="pi pi-calendar" [severity]="bookingDetailTab === 'booking' ? 'primary' : 'contrast'" (click)="bookingDetailTab = 'booking'"></button>
                    <button pButton type="button" label="Product Details" icon="pi pi-box" [severity]="bookingDetailTab === 'products' ? 'primary' : 'contrast'" (click)="bookingDetailTab = 'products'"></button>
                    <button pButton type="button" label="Payment Details" icon="pi pi-receipt" [severity]="bookingDetailTab === 'payment' ? 'primary' : 'contrast'" (click)="bookingDetailTab = 'payment'"></button>
                    <button pButton type="button" label="History" icon="pi pi-history" [severity]="bookingDetailTab === 'history' ? 'primary' : 'contrast'" (click)="bookingDetailTab = 'history'; loadSnapshots()"></button>
                </div>

                <article class="booking-invoice-panel" *ngIf="selectedBooking && bookingDetailTab === 'booking'">
                    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
                        <div>
                            <h3 class="font-display text-2xl mt-0 mb-1">Booking Details</h3>
                            <p class="text-muted-color m-0">Review the actual booking row first, then edit or clone it into the extension fields.</p>
                        </div>
                        <div class="booking-invoice-actions">
                            <button pButton type="button" icon="pi pi-pencil" label="Edit" severity="info" (click)="editBooking()" [disabled]="bookingActionSaving()"></button>
                            <button pButton type="button" icon="pi pi-copy" label="Clone" severity="help" (click)="cloneBooking()" [disabled]="bookingActionSaving()"></button>
                            <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="warn" (click)="refreshBooking()" [disabled]="bookingActionSaving()"></button>
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
                            <label class="booking-stack-field">
                                <span class="font-semibold">Booking ID</span>
                                <input pInputText [ngModel]="bookingForm.reservationId ?? 'New'" name="bookingId" readonly />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Reference</span>
                                <input pInputText [(ngModel)]="bookingForm.reference" name="bookingReference" />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Voucher Code</span>
                                <input pInputText [(ngModel)]="bookingForm.voucherCode" name="bookingVoucherCode" />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Customer ID</span>
                                <input pInputText type="number" [(ngModel)]="bookingForm.customerId" name="bookingCustomerId" />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Account ID</span>
                                <input pInputText type="number" [(ngModel)]="bookingForm.accountId" name="bookingAccountId" />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Client Email</span>
                                <input pInputText type="email" [(ngModel)]="bookingForm.customerEmail" name="bookingCustomerEmail" />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Currency</span>
                                <input pInputText [(ngModel)]="bookingForm.currency" name="bookingCurrency" />
                            </label>
                            <label class="booking-stack-field">
                                <span class="font-semibold">Total Amount</span>
                                <input pInputText type="number" min="0" step="0.01" [(ngModel)]="bookingForm.totalAmount" name="bookingTotalAmount" />
                            </label>
                            <label class="booking-stack-field">
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
                            <h3 class="font-display text-2xl mt-0 mb-1">Service Details</h3>
                            <p class="text-muted-color m-0">Add, view, or remove services under one service type/category on this booking.</p>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-6 gap-2 mb-3">
                        <p-select [options]="serviceTypeOptions()" [(ngModel)]="detailSelectedType" optionLabel="label" optionValue="value" placeholder="Service Type / Category" (onChange)="detailSelectedService = null" appendTo="body"></p-select>
                        <p-select [options]="detailFilteredServices()" [(ngModel)]="detailSelectedService" optionLabel="serviceLabel" placeholder="Select service" [filter]="true" appendTo="body"></p-select>
                        <p-select [options]="suppliers()" [(ngModel)]="detailSupplierId" optionLabel="name" optionValue="supplierId" placeholder="Supplier" [showClear]="true" [filter]="true" appendTo="body"></p-select>
                        <input pInputText type="number" min="1" [(ngModel)]="detailSelectedQty" placeholder="Qty" />
                        <input pInputText type="number" min="0" max="100" step="0.01" [(ngModel)]="detailDiscountPct" placeholder="Disc %" />
                        <button pButton type="button" icon="pi pi-plus" label="Add Service" severity="success" (click)="addServiceToExistingBooking()" [loading]="productSaving()"></button>
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

                    <p class="text-red-500 mb-2" *ngIf="productStatus()">{{ productStatus() }}</p>

                    <p-table [value]="selectedBookingServices()" styleClass="p-datatable-sm booking-display-table" [tableStyle]="{ 'min-width': '100%' }" [rows]="10" [paginator]="selectedBookingServices().length > 10">
                        <ng-template pTemplate="header">
                            <tr><th>Category</th><th>Service</th><th>Supplier</th><th>Qty</th><th>Adults</th><th>Rooms</th><th>Nights</th><th>Disc%</th><th>Unit</th><th>Total</th><th class="text-right w-20">Actions</th></tr>
                        </ng-template>
                        <ng-template pTemplate="body" let-item>
                            <tr>
                                <td>{{ serviceTypeLabel(item.serviceType) }}</td>
                                <td>{{ display(item.serviceName) }}</td>
                                <td>{{ supplierName(item.supplierId) }}</td>
                                <td>{{ display(item.quantity) }}</td>
                                <td>{{ display(item.adultPax) }}</td>
                                <td>{{ display(item.rooms) }}</td>
                                <td>{{ display(item.nights) }}</td>
                                <td>{{ item.discountPercent != null ? item.discountPercent + '%' : '-' }}</td>
                                <td>{{ money(item.unitPrice || 0, item.currency || bookingForm.currency) }}</td>
                                <td class="font-semibold">{{ money(effectiveTotal(item), item.currency || bookingForm.currency) }}</td>
                                <td class="text-right">
                                    <button pButton type="button" icon="pi pi-trash" severity="danger" rounded text class="app-row-action" pTooltip="Remove" (click)="removeServiceFromExistingBooking(item)" [loading]="productSaving()"></button>
                                </td>
                            </tr>
                        </ng-template>
                        <ng-template pTemplate="emptymessage">
                            <tr><td colspan="11" class="text-center text-muted-color py-4">No service rows captured. Use the form above to add services.</td></tr>
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
                            <button pButton type="button" icon="pi pi-refresh" label="Refresh" severity="warn" (click)="refreshInvoice()" [disabled]="invoiceLoading()"></button>
                            <button pButton type="button" icon="pi pi-plus" label="New" (click)="prepareNewInvoice()"></button>
                        </div>
                    </div>

                    <form class="booking-invoice-form">
                        <label class="booking-stack-field">
                            <span class="font-semibold">Invoice ID</span>
                            <input pInputText [ngModel]="invoiceForm.id ?? 'New'" name="invoiceId" readonly />
                        </label>
                        <label class="booking-stack-field">
                            <span class="font-semibold">Reservation ID</span>
                            <input pInputText type="number" [(ngModel)]="invoiceForm.reservationId" name="invoiceReservationId" />
                        </label>
                        <label class="booking-stack-field">
                            <span class="font-semibold">Total Amount</span>
                            <input pInputText type="number" min="0" step="0.01" [(ngModel)]="invoiceForm.totalAmount" name="invoiceTotalAmount" />
                        </label>
                        <label class="booking-stack-field">
                            <span class="font-semibold">Currency</span>
                            <input pInputText [(ngModel)]="invoiceForm.currency" name="invoiceCurrency" />
                        </label>
                        <label class="booking-stack-field">
                            <span class="font-semibold">Status</span>
                            <p-select [options]="invoiceStatusOptions" [(ngModel)]="invoiceForm.status" name="invoiceStatus" optionLabel="label" optionValue="value" appendTo="body"></p-select>
                        </label>
                        <label class="booking-stack-field">
                            <span class="font-semibold">Issued At</span>
                            <input pInputText [ngModel]="display(invoiceForm.issuedAt)" name="invoiceIssuedAt" readonly />
                        </label>
                    </form>

                    <p class="text-red-500 mt-3 mb-0" *ngIf="invoiceStatus()">{{ invoiceStatus() }}</p>
                </article>

                <article class="booking-invoice-panel" *ngIf="bookingDetailTab === 'history'">
                    <h3 class="font-display text-2xl mt-0 mb-3">Booking History</h3>
                    <div *ngIf="bookingHistoryLoading()" class="flex justify-center py-6"><i class="pi pi-spin pi-spinner text-3xl text-primary"></i></div>
                    <div *ngIf="!bookingHistoryLoading() && snapshots().length === 0" class="text-muted-color text-center py-4">No history snapshots found.</div>
                    <div class="grid gap-3" *ngIf="!bookingHistoryLoading() && snapshots().length > 0">
                        <div *ngFor="let snap of snapshots()" class="flex gap-3 items-start border border-surface rounded-lg p-3">
                            <i class="pi pi-history text-primary mt-1"></i>
                            <div>
                                <strong>{{ snap.snapshotType }}</strong>
                                <span class="text-muted-color text-sm ml-2">{{ snap.createdAt | date:'MM/dd/yyyy HH:mm' }}</span>
                                <p class="m-0 text-sm" *ngIf="snap.createdBy">by {{ snap.createdBy }}</p>
                            </div>
                        </div>
                    </div>
                </article>

                <ng-template pTemplate="footer">
                    <div class="booking-detail-footer">
                        <button pButton type="button" label="Close" severity="contrast" (click)="bookingDetailVisible = false"></button>
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
                    <button pButton type="button" label="View Dashboard" severity="info" (click)="goDashboard()"></button>
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
            padding: 1rem 1.15rem 1.15rem;
        }

        .booking-modal-body {
            display: grid;
            grid-template-columns: minmax(9.5rem, 11rem) minmax(0, 1fr);
            gap: 1rem;
            align-items: start;
        }

        .booking-modal-steps {
            display: grid;
            gap: 0.75rem;
            position: sticky;
            top: 0;
        }

        :host ::ng-deep .booking-step-nav.p-button {
            justify-content: flex-start;
            width: 100%;
        }

        .booking-modal-main {
            display: grid;
            gap: 1rem;
            min-width: 0;
        }

        .booking-modal-intro {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 0.85rem;
            padding: 0.85rem 1rem;
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
            font-size: 1.25rem;
        }

        .booking-step-strip {
            display: none;
        }

        .booking-services-header {
            margin-bottom: 1rem;
        }

        .booking-services-toolbar {
            display: grid;
            grid-template-columns: repeat(3, minmax(0, 1fr));
            gap: 1rem 1.25rem;
            margin-bottom: 1rem;
        }

        .booking-stack-field-span {
            grid-column: 1 / -1;
        }

        .booking-line-details {
            display: grid;
            grid-template-columns: repeat(3, minmax(0, 1fr));
            gap: 1rem 1.25rem;
            padding: 1rem;
            margin-bottom: 1rem;
            border: 1px dashed var(--surface-border);
            border-radius: 0.75rem;
            background: color-mix(in srgb, var(--surface-card), var(--surface-ground) 18%);
        }

        .booking-services-add {
            align-self: end;
        }

        :host ::ng-deep .booking-step-button.p-button,
        .booking-modal-actions .p-button {
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
            gap: 1rem 1.5rem;
            align-content: start;
        }

        .booking-stack-field {
            display: grid;
            gap: 0.5rem;
            min-width: 0;
        }

        .booking-stack-field-wide {
            grid-column: 1 / -1;
        }

        .booking-stack-field span {
            font-size: 0.9rem;
            line-height: 1.3;
        }

        .booking-stack-field .p-inputtext,
        .booking-stack-field p-select,
        .booking-stack-field p-multiselect {
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

            .booking-modal-intro,
            .booking-modal-footer {
                flex-direction: column;
                align-items: stretch;
            }

            .booking-modal-body {
                grid-template-columns: 1fr;
            }

            .booking-modal-steps {
                grid-template-columns: repeat(3, minmax(0, 1fr));
            }

            .booking-services-toolbar,
            .booking-line-details {
                grid-template-columns: 1fr;
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
            .booking-stack-field,
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
    readonly grossTotal = computed(() =>
        this.serviceItems().reduce((sum, item) => sum + Number(item.unitPrice) * Number(item.quantity), 0)
    );
    readonly totalDiscount = computed(() =>
        this.serviceItems().reduce((sum, item) => {
            const gross = Number(item.unitPrice) * Number(item.quantity);
            const disc = Number(item.discountPercent ?? 0);
            return sum + (gross * disc / 100);
        }, 0)
    );
    readonly netTotal = computed(() => this.grossTotal() - this.totalDiscount());
    readonly total = computed(() => this.netTotal());
    readonly filteredServices = computed(() => this.services().filter((item) => item.serviceType === this.selectedType));
    readonly groupedFilteredServices = computed(() => {
        const groups = new Map<string, ServiceOption[]>();
        for (const item of this.filteredServices()) {
            const label = item.categoryName || 'Uncategorized';
            if (!groups.has(label)) groups.set(label, []);
            groups.get(label)!.push(item);
        }
        return Array.from(groups.entries())
            .sort(([a], [b]) => a.localeCompare(b))
            .map(([label, items]) => ({ label, items }));
    });
    readonly groupedServiceOptions = computed(() => this.serviceTypeOptions().map((type) => ({
        label: type.label,
        value: type.value,
        items: this.services().filter((item) => item.serviceType === type.value)
    })));
    readonly pageSize = SYSTEM_PAGE_SIZE;
    readonly serviceTypeOptions = signal<ServiceTypeOption[]>([]);
    readonly paymentMethods = signal<PaymentMethodOption[]>([]);
    readonly tripAccounts = signal<TripAccountOption[]>([]);
    readonly consultants = signal<any[]>([]);
    readonly suppliers = signal<SupplierOption[]>([]);
    readonly currencyOptions = signal<Array<{ code: string; name: string }>>([
        { code: 'USD', name: 'US Dollar' }, { code: 'EUR', name: 'Euro' }, { code: 'GBP', name: 'British Pound' },
        { code: 'ZAR', name: 'South African Rand' }, { code: 'BWP', name: 'Botswana Pula' }, { code: 'ZMW', name: 'Zambian Kwacha' }
    ]);
    readonly snapshots = signal<any[]>([]);
    readonly bookingHistoryLoading = signal(false);
    readonly productSaving = signal(false);
    readonly productStatus = signal('');
    readonly detailFilteredServices = computed(() => this.services().filter((item) => item.serviceType === this.detailSelectedType));

    customerSearch = '';
    selectedCustomer: Customer | null = null;
    client: Customer = { durationOfStayDays: 0 };
    selectedType: 'Product' | 'Accommodation' | 'Activity' | 'Transfer' | 'Tour' | 'Combo' = 'Product';
    // Effective FX rates (foreign units per 1 USD) used to preview converted prices in the wizard.
    private ratesMap: Record<string, number> = { USD: 1 };
    selectedServices: ServiceOption[] = [];
    selectedQuantity = 1;
    selectedTripAccountId: number | null = null;
    selectedConsultantId: number | null = null;
    agencyVoucherRef = '';
    clientCountry = '';
    bookingNotes = '';
    transactionReference = '';
    proofOfPaymentUrl = '';
    bookingPaymentStatus = 'NotPaid';
    bookingStatusFilter = '';
    lineAdultPax: number | null = null;
    lineChildPax: number | null = null;
    lineCompPax: number | null = null;
    lineRooms: number | null = null;
    lineNights: number | null = null;
    linePickup = '';
    lineDropoff = '';
    lineActivityDate = '';
    lineDiscountPercent: number | null = null;
    lineSupplierId: number | null = null;
    detailSelectedType: 'Product' | 'Accommodation' | 'Activity' | 'Transfer' | 'Tour' = 'Product';
    detailSelectedService: ServiceOption | null = null;
    detailSelectedQty = 1;
    detailDiscountPct: number | null = null;
    detailSupplierId: number | null = null;
    paymentMethod = 'Manual';
    currency = 'USD';
    resultVisible = false;
    bookingModalVisible = false;
    bookingDetailVisible = false;
    bookingSearch = '';
    isTripAccountsPage = false;
    selectedBooking: Record<string, unknown> | null = null;
    readonly selectedVoucher = signal<Record<string, unknown> | null>(null);
    bookingDetailTab: 'booking' | 'products' | 'payment' | 'history' = 'booking';
    bookingForm: BookingForm = this.emptyBookingForm();
    invoiceForm: InvoiceForm = this.emptyInvoiceForm();
    readonly bookingStatusOptions = [
        { label: 'Enquiry', value: 'Enquiry' },
        { label: 'Provisional', value: 'Provisional' },
        { label: 'Confirmed', value: 'Confirmed' },
        { label: 'Closed', value: 'Closed' },
        { label: 'Cancelled', value: 'Cancelled' }
    ];
    readonly invoiceStatusOptions = [
        { label: 'Pending', value: 'Pending' },
        { label: 'Paid', value: 'Paid' },
        { label: 'Cancelled', value: 'Cancelled' },
        { label: 'Void', value: 'Void' }
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
        const search = this.bookingSearch.trim();
        const statusFilter = this.bookingStatusFilter;

        const handleResult = (items: Record<string, unknown>[], totalCount: number, resultPage: number) => {
            let rows = items ?? [];
            if (statusFilter) {
                rows = rows.filter((row) => String(row['status'] ?? '') === statusFilter);
            }
            this.bookings.set(rows);
            this.bookingTotalRecords.set(statusFilter ? rows.length : (totalCount ?? rows.length));
            this.bookingFirst.set(((resultPage ?? page) - 1) * this.pageSize);
            this.bookingLoading.set(false);
        };

        if (this.auth.isAdmin()) {
            this.api.listPage<Record<string, unknown>>('/api/reservations', page, this.pageSize, search).subscribe({
                next: (result) => handleResult(result.items ?? [], result.totalCount ?? 0, result.page ?? page),
                error: (err) => {
                    this.bookingLoading.set(false);
                    this.status.set(this.describeError(err));
                }
            });
            return;
        }

        const accountId = this.auth.user()?.id;
        if (!accountId) {
            this.bookings.set([]);
            this.bookingTotalRecords.set(0);
            this.bookingLoading.set(false);
            return;
        }

        this.api.listMinePage<Record<string, unknown>>('/api/reservations/mine', accountId, page, this.pageSize, search).subscribe({
            next: (result) => handleResult(result.items ?? [], result.totalCount ?? 0, result.page ?? page),
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
        this.cancelBookingFromTable(booking);
    }

    checkStatus(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        if (!id) {
            this.messages.add({ severity: 'warn', summary: 'No ID', detail: 'Cannot check status — reservation ID missing.' });
            return;
        }
        this.api.get(`/api/reservations/${id}`).subscribe({
            next: (res: any) => {
                const data = res?.data ?? res;
                const ref = data?.reference ?? data?.voucherCode ?? id;
                const live = data?.status ?? 'Unknown';
                this.messages.add({ severity: 'info', summary: `Booking ${ref}`, detail: `Live status: ${live}`, life: 5000 });
            },
            error: () => this.messages.add({ severity: 'error', summary: 'Check failed', detail: 'Could not retrieve live booking status.' })
        });
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
            agencyConsultantId: this.optionalNumber(this.bookingForm.agencyConsultantId),
            agencyVoucherReference: this.bookingForm.agencyVoucherReference || null,
            currency: this.bookingForm.currency,
            totalAmount: Number(this.bookingForm.totalAmount),
            status: this.bookingForm.status,
            customerEmail: this.bookingForm.customerEmail || null,
            country: this.bookingForm.country || null,
            notes: this.bookingForm.notes || null,
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
        const reservationId = this.reservationIdFromSelectedBooking();
        const total = Number(this.selectedBooking?.['totalAmount'] ?? 0);
        const currency = String(this.selectedBooking?.['currency'] ?? this.selectedBooking?.['currencyCode'] ?? 'USD');
        this.invoiceForm = {
            id: null,
            reservationId,
            totalAmount: total > 0 ? total : 0,
            currency,
            status: 'Pending'
        };
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

    onServiceTypeChange(): void {
        // Reset the service picker when the category changes so it lists the new type.
        this.selectedServices = [];
        this.status.set('');
    }

    selectGroupedService(service: ServiceOption): void {
        this.selectedType = service.serviceType;
        this.selectedServices = [service];
        this.status.set('');
    }

    addService(): void {
        if (!this.selectedServices.length || this.selectedQuantity < 1) {
            this.status.set('Select at least one service and a quantity.');
            return;
        }

        const unpriced = this.selectedServices.filter((s) => !s.hasSelectedCurrencyPrice);
        if (unpriced.length) {
            this.status.set(`No active ${this.currency.toUpperCase()} price is configured for: ${unpriced.map((s) => s.serviceName).join(', ')}.`);
            return;
        }

        const totalPax = Number(this.lineAdultPax ?? 0) + Number(this.lineChildPax ?? 0);
        for (const service of this.selectedServices) {
            if (service.maxPax && totalPax > service.maxPax) {
                this.status.set(`${service.serviceName} accommodates at most ${service.maxPax} guests.`);
                return;
            }
            if ((service.serviceType === 'Activity' || service.serviceType === 'Transfer' || service.serviceType === 'Tour') && !this.lineActivityDate) {
                this.status.set(`Set an activity date for ${service.serviceName} so special pricing applies for that period.`);
                return;
            }
        }

        const quantity = Number(this.selectedQuantity);
        const disc = Number(this.lineDiscountPercent ?? 0);

        // Bulk-add every selected service. The shared line settings (qty, pax, dates,
        // discount, supplier) apply to each added row.
        for (const service of this.selectedServices) {
            const unitPrice = Number(service.unitPrice);
            const grossTotal = quantity * unitPrice;
            const discountAmount = grossTotal * disc / 100;
            const item: BookingServiceItem = {
                serviceType: service.serviceType === 'Combo' ? 'Combo' : service.serviceType,
                serviceId: service.serviceId,
                serviceName: service.serviceName,
                productKind: service.productKind ?? (service.serviceType === 'Combo' ? 'Combo' : 'Standard'),
                comboId: service.comboId ?? (service.serviceType === 'Combo' ? service.serviceId : undefined),
                quantity,
                unitPrice,
                totalPrice: grossTotal - discountAmount,
                currency: service.currency,
                supplierId: this.lineSupplierId,
                adultPax: this.lineAdultPax,
                childPax: this.lineChildPax,
                compPax: this.lineCompPax,
                rooms: this.lineRooms,
                nights: this.lineNights,
                pickupLocation: this.linePickup || null,
                dropoffLocation: this.lineDropoff || null,
                activityDate: this.lineActivityDate || null,
                discountPercent: disc > 0 ? disc : null
            };
            this.currency = item.currency;
            this.serviceItems.update((rows) => this.upsertServiceItem(rows, item));
        }

        this.selectedServices = [];
        this.selectedQuantity = 1;
        this.lineAdultPax = null;
        this.lineChildPax = null;
        this.lineCompPax = null;
        this.lineRooms = null;
        this.lineNights = null;
        this.linePickup = '';
        this.lineDropoff = '';
        this.lineActivityDate = '';
        this.lineDiscountPercent = null;
        this.lineSupplierId = null;
        this.status.set('');
    }

    private upsertServiceItem(rows: BookingServiceItem[], item: BookingServiceItem): BookingServiceItem[] {
        const index = rows.findIndex((row) =>
            row.serviceType === item.serviceType
            && row.serviceId === item.serviceId
            && row.currency === item.currency
            && Number(row.supplierId ?? 0) === Number(item.supplierId ?? 0)
            && Number(row.discountPercent ?? 0) === Number(item.discountPercent ?? 0)
        );
        if (index < 0) return [...rows, item];

        return rows.map((row, rowIndex) => {
            if (rowIndex !== index) return row;
            const quantity = Number(row.quantity) + Number(item.quantity);
            const gross = quantity * Number(row.unitPrice);
            const discount = Number(row.discountPercent ?? 0);
            return {
                ...row,
                quantity,
                adultPax: this.sumNullable(row.adultPax, item.adultPax),
                childPax: this.sumNullable(row.childPax, item.childPax),
                compPax: this.sumNullable(row.compPax, item.compPax),
                rooms: this.sumNullable(row.rooms, item.rooms),
                nights: row.nights ?? item.nights,
                totalPrice: gross - (gross * discount / 100)
            };
        });
    }

    private sumNullable(a?: number | null, b?: number | null): number | null {
        const left = Number(a ?? 0);
        const right = Number(b ?? 0);
        const sum = left + right;
        return sum > 0 ? sum : null;
    }

    onCurrencyChange(value: string): void {
        this.currency = (value || 'USD').toUpperCase();
        this.services.update((rows) => rows.map((item) => this.withSelectedCurrencyPrice(item)));
        this.serviceItems.update((rows) => rows.map((item) => {
            const service = this.services().find((option) => option.serviceType === item.serviceType && option.serviceId === item.serviceId);
            if (!service?.hasSelectedCurrencyPrice) return item;
            return {
                ...item,
                unitPrice: service.unitPrice,
                currency: service.currency,
                totalPrice: Number(item.quantity) * service.unitPrice
            };
        }));
        this.selectedServices = this.selectedServices.map((s) => this.withSelectedCurrencyPrice(s));
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
        this.submitWithStatus('Confirmed');
    }

    submitWithStatus(status: string): void {
        if (!this.validateStep(0) || !this.validateStep(1) || !this.validateStep(2)) return;
        const accountId = this.auth.user()?.id;
        const netTotal = this.netTotal();
        const request: BookingCheckoutRequest = {
            customerId: this.selectedCustomer?.id ?? null,
            tripAccountId: this.selectedTripAccountId,
            customer: this.selectedCustomer?.id ? null : { ...this.client, agentCompanyId: this.selectedTripAccountId },
            reservation: {
                reference: this.agencyVoucherRef,
                accountId,
                agencyId: this.selectedTripAccountId,
                agencyConsultantId: this.selectedConsultantId,
                agencyVoucherReference: this.agencyVoucherRef || null,
                customerId: this.selectedCustomer?.id ?? null,
                currency: this.currency,
                totalAmount: netTotal,
                status,
                customerEmail: this.client.email ?? null,
                country: this.clientCountry || null,
                notes: this.bookingNotes || null
            },
            invoice: {
                totalAmount: netTotal,
                currency: this.currency,
                status: 'Pending'
            },
            payment: {
                method: this.paymentMethod,
                amount: netTotal,
                currencyCode: this.currency,
                idempotencyKey: `booking-${Date.now()}-${Math.random().toString(16).slice(2)}`,
                transactionReference: this.transactionReference?.trim() || undefined
            },
            proofOfPaymentUrl: this.proofOfPaymentUrl?.trim() || undefined,
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
                this.messages.add({ severity: 'success', summary: `Booking Saved (${status})`, detail: 'Reservation, invoice, and voucher captured.' });
            },
            error: (err) => {
                this.submitting.set(false);
                this.status.set(this.describeError(err));
            }
        });
    }

    closeBookingFromTable(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        if (!id || !confirm('Close this booking?')) return;
        this.api.post(`/api/reservations/${id}/close`, {}).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Closed', detail: `Booking #${id} closed.` });
                this.loadBookings(this.currentBookingPage());
            },
            error: (err) => this.messages.add({ severity: 'error', summary: 'Failed', detail: this.describeError(err) })
        });
    }

    cancelBookingFromTable(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        if (!id || !confirm('Cancel this booking?')) return;
        this.api.post(`/api/reservations/${id}/cancel`, {}).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Cancelled', detail: `Booking #${id} cancelled.` });
                this.loadBookings(this.currentBookingPage());
            },
            error: (err) => this.messages.add({ severity: 'error', summary: 'Failed', detail: this.describeError(err) })
        });
    }

    openBookingFromTable(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        if (!id) return;
        this.api.post(`/api/reservations/${id}/open`, {}).subscribe({
            next: () => {
                this.messages.add({ severity: 'success', summary: 'Re-opened', detail: `Booking #${id} is now Confirmed.` });
                this.loadBookings(this.currentBookingPage());
            },
            error: (err) => this.messages.add({ severity: 'error', summary: 'Failed', detail: this.describeError(err) })
        });
    }

    cloneBookingFromTable(booking: Record<string, unknown>): void {
        const id = Number(booking['reservationId'] ?? booking['id']);
        if (!id) return;
        this.api.post(`/api/reservations/${id}/clone`, {}).subscribe({
            next: (clone: any) => {
                this.messages.add({ severity: 'success', summary: 'Cloned', detail: `New booking #${clone?.reservationId ?? ''} created as Enquiry.` });
                this.loadBookings(1);
            },
            error: (err) => this.messages.add({ severity: 'error', summary: 'Failed', detail: this.describeError(err) })
        });
    }

    loadSnapshots(): void {
        const id = this.reservationIdFromSelectedBooking();
        if (!id) return;
        this.bookingHistoryLoading.set(true);
        this.snapshots.set([]);
        this.api.get<any[]>(`/api/reservations/${id}/snapshots`).subscribe({
            next: (rows) => { this.snapshots.set(rows ?? []); this.bookingHistoryLoading.set(false); },
            error: () => this.bookingHistoryLoading.set(false)
        });
    }

    addServiceToExistingBooking(): void {
        const reservationId = this.reservationIdFromSelectedBooking();
        if (!reservationId) {
            this.productStatus.set('No reservation ID available for this booking.');
            return;
        }
        if (!this.detailSelectedService || this.detailSelectedQty < 1) {
            this.productStatus.set('Select a service and quantity.');
            return;
        }
        if (!this.detailSelectedService.hasSelectedCurrencyPrice) {
            this.productStatus.set(`No active price is configured for ${this.detailSelectedService.serviceName}.`);
            return;
        }

        const quantity = Number(this.detailSelectedQty);
        const unitPrice = Number(this.detailSelectedService.unitPrice);
        const disc = Number(this.detailDiscountPct ?? 0);
        const gross = quantity * unitPrice;
        const duplicate = this.selectedBookingServices().some((item) =>
            item.serviceType === this.detailSelectedService?.serviceType
            && Number(item.serviceId) === Number(this.detailSelectedService?.serviceId)
            && Number(item.supplierId ?? 0) === Number(this.detailSupplierId ?? 0)
        );
        if (duplicate) {
            this.productStatus.set('This service is already on the booking. Remove or edit the existing row instead of adding a duplicate.');
            return;
        }
        const payload = {
            serviceType: this.detailSelectedService.serviceType,
            serviceId: this.detailSelectedService.serviceId,
            serviceName: this.detailSelectedService.serviceName,
            quantity,
            unitPrice,
            totalPrice: gross - (gross * disc / 100),
            currency: this.detailSelectedService.currency,
            supplierId: this.detailSupplierId,
            discountPercent: disc > 0 ? disc : null
        };

        this.productSaving.set(true);
        this.productStatus.set('');
        this.api.post(`/api/reservations/${reservationId}/service-items`, payload).subscribe({
            next: () => {
                this.productSaving.set(false);
                this.detailSelectedService = null;
                this.detailSelectedQty = 1;
                this.detailDiscountPct = null;
                this.detailSupplierId = null;
                this.loadBookingDetails();
                this.loadBookings(this.currentBookingPage());
                this.messages.add({ severity: 'success', summary: 'Service added', detail: 'Service line added to the booking.' });
            },
            error: (err) => {
                this.productSaving.set(false);
                this.productStatus.set(this.describeError(err));
            }
        });
    }

    removeServiceFromExistingBooking(item: any): void {
        const reservationId = this.reservationIdFromSelectedBooking();
        const itemId = Number(item?.id);
        if (!reservationId || !itemId) {
            this.productStatus.set('Cannot remove this service line.');
            return;
        }
        if (!confirm('Remove this service line from the booking?')) return;

        this.productSaving.set(true);
        this.productStatus.set('');
        this.api.delete(`/api/reservations/${reservationId}/service-items/${itemId}`).subscribe({
            next: () => {
                this.productSaving.set(false);
                this.loadBookingDetails();
                this.loadBookings(this.currentBookingPage());
                this.messages.add({ severity: 'success', summary: 'Service removed', detail: 'Service line removed from the booking.' });
            },
            error: (err) => {
                this.productSaving.set(false);
                this.productStatus.set(this.describeError(err));
            }
        });
    }

    effectiveTotal(item: BookingServiceItem): number {
        const gross = Number(item.unitPrice) * Number(item.quantity);
        const disc = Number(item.discountPercent ?? 0);
        return gross - (gross * disc / 100);
    }

    paymentStatusSeverity(status: unknown): 'success' | 'info' | 'warn' | 'danger' | 'contrast' {
        const s = String(status ?? '').toLowerCase();
        if (s.includes('fullypaid') || s === 'paid') return 'success';
        if (s.includes('deposited')) return 'info';
        if (s.includes('shortfall')) return 'danger';
        return 'warn';
    }

    travelStatusSeverity(status: unknown): 'success' | 'info' | 'warn' | 'contrast' {
        const s = String(status ?? '').toLowerCase();
        if (s.includes('travelled')) return 'success';
        if (s.includes('inprogress') || s.includes('in progress')) return 'info';
        return 'warn';
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
        // The list payload doesn't include service items; the invoice/header total is the source of truth.
        const total = Number(booking['invoiceTotalAmount'] ?? booking['totalAmount'] ?? 0);
        const paid = Number(booking['paymentAmount'] ?? 0);
        const due = total - paid;
        const prefix = due > 0 ? '+' : '';
        const currency = String(booking['invoiceCurrency'] ?? booking['currency'] ?? 'USD');
        return `${prefix}${this.money(due, currency)}`;
    }

    supplierName(supplierId: unknown): string {
        const id = Number(supplierId);
        if (!Number.isFinite(id) || id <= 0) return '-';
        return this.suppliers().find((supplier) => supplier.supplierId === id)?.name ?? `Supplier #${id}`;
    }

    serviceTypeLabel(value: unknown): string {
        const raw = String(value ?? '');
        return this.serviceTypeOptions().find((t) => t.value === raw)?.label ?? raw;
    }

    statusSeverity(value: unknown): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
        const status = String(value ?? '').toLowerCase();
        if (status === 'paid' || status === 'completed' || status === 'captured') return 'success';
        if (status === 'confirmed' || status === 'travelled' || status === 'checkedin') return 'success';
        if (status === 'pending' || status === 'provisional') return 'warn';
        if (status === 'enquiry') return 'info';
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
        if (['serviceType', 'category', 'productType'].includes(value)) return 'Service Type / Category';
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
        this.selectedServices = [];
        this.selectedQuantity = 1;
        this.selectedTripAccountId = null;
        this.selectedConsultantId = null;
        this.agencyVoucherRef = '';
        this.clientCountry = '';
        this.bookingNotes = '';
        this.transactionReference = '';
        this.proofOfPaymentUrl = '';
        this.bookingPaymentStatus = 'NotPaid';
        this.lineSupplierId = null;
        this.detailSupplierId = null;
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
            reservationId: null, reference: '', voucherCode: '',
            customerId: null, accountId: null, customerEmail: '',
            currency: 'USD', totalAmount: 0, status: 'Enquiry',
            closingByUserName: '', agencyConsultantId: null,
            agencyVoucherReference: '', country: '',
            paymentStatus: 'NotPaid', travelStatus: 'NotCheckedIn', notes: ''
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
            status: String(booking?.['status'] ?? 'Enquiry'),
            closingByUserName: String(booking?.['closingByUserName'] ?? ''),
            agencyConsultantId: this.toNullableNumber(booking?.['agencyConsultantId']),
            agencyVoucherReference: String(booking?.['agencyVoucherReference'] ?? ''),
            country: String(booking?.['country'] ?? ''),
            paymentStatus: String(booking?.['paymentStatus'] ?? 'NotPaid'),
            travelStatus: String(booking?.['travelStatus'] ?? 'NotCheckedIn'),
            notes: String(booking?.['notes'] ?? '')
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
        this.loadExchangeRatesMap();
        forkJoin({
            // Fixed metadata arrays are not paginated; reference lists and catalogues are loaded in
            // full via listAll so dropdown search is never capped at the default page size.
            serviceTypes: this.api.list<ServiceTypeOption>('/api/frontend/service-types'),
            paymentMethods: this.api.list<PaymentMethodOption>('/api/frontend/payment-methods'),
            tripAccounts: this.api.listAll<TripAccountOption>('/api/trip-accounts'),
            consultants: this.api.listAll<any>('/api/consultants'),
            suppliers: this.api.listAll<SupplierOption>('/api/suppliers'),
            currencies: this.api.listAll<any>('/api/currencies'),
            products: this.api.listAll<any>('/api/products'),
            accommodations: this.api.listAll<any>('/api/accommodations'),
            activities: this.api.listAll<any>('/api/activities'),
            transfers: this.api.listAll<any>('/api/transfers'),
            tours: this.api.listAll<any>('/api/tours'),
            combos: this.api.listAll<any>('/api/combos')
        }).subscribe({
            next: (rows) => {
                this.serviceTypeOptions.set(rows.serviceTypes);
                this.paymentMethods.set(rows.paymentMethods);
                this.tripAccounts.set(rows.tripAccounts);
                this.consultants.set(rows.consultants.map((c: any) => ({ ...c, fullName: `${c.firstName ?? ''} ${c.lastName ?? ''}`.trim() || c.email || `Consultant #${c.id}` })));
                this.suppliers.set((rows.suppliers ?? []).map((s: any) => ({ ...s, supplierId: Number(s.supplierId ?? s.id), name: s.name ?? `Supplier #${s.supplierId ?? s.id}` })));
                if (Array.isArray(rows.currencies) && rows.currencies.length) {
                    this.currencyOptions.set(rows.currencies
                        .filter((c: any) => c.isActive !== false && c.code)
                        .map((c: any) => ({ code: c.code, name: c.name ?? c.code })));
                }
                this.selectedType = rows.serviceTypes[0]?.value ?? 'Product';
                this.paymentMethod = rows.paymentMethods[0]?.value ?? '';
                this.services.set([
                    ...rows.products.map((item) => this.toServiceOption('Product', item)),
                    ...rows.accommodations.map((item) => this.toServiceOption('Accommodation', item)),
                    ...rows.activities.map((item) => this.toServiceOption('Activity', item)),
                    ...rows.transfers.map((item) => this.toServiceOption('Transfer', item)),
                    ...rows.tours.map((item) => this.toServiceOption('Tour', item)),
                    ...(rows.combos ?? []).map((item: any) => this.toComboOption(item))
                ]);
            },
            error: (err) => {
                this.services.set([]);
                this.serviceTypeOptions.set([]);
                this.paymentMethods.set([]);
                this.tripAccounts.set([]);
                this.suppliers.set([]);
                this.status.set(this.describeError(err));
            }
        });
    }

    private toServiceOption(serviceType: 'Product' | 'Accommodation' | 'Activity' | 'Transfer' | 'Tour', item: any): ServiceOption {
        const basePriceUsd = Number(item.basePriceUsd ?? item.price ?? 0);
        const option: ServiceOption = {
            serviceType,
            serviceId: Number(item.productId ?? item.id),
            serviceName: item.name ?? item.type ?? item.productName ?? serviceType,
            description: item.description ?? item.category ?? '',
            categoryName: item.categoryName ?? item.category ?? 'Uncategorized',
            maxPax: item.maxPax ?? item.capacity ?? item.guestCapacity ?? null,
            unitPrice: basePriceUsd,
            basePriceUsd,
            currency: 'USD',
            serviceLabel: '',
            hasSelectedCurrencyPrice: false,
            prices: Array.isArray(item.prices) ? item.prices : []
        };
        return this.withSelectedCurrencyPrice(option);
    }

    private toComboOption(item: any): ServiceOption {
        const option: ServiceOption = {
            serviceType: 'Combo',
            serviceId: Number(item.id),
            comboId: Number(item.id),
            productKind: 'Combo',
            serviceName: item.name ?? `Combo #${item.id}`,
            description: item.description ?? item.code ?? '',
            categoryName: 'Combos',
            maxPax: item.maxProducts ?? null,
            unitPrice: 0,
            basePriceUsd: 0,
            currency: 'USD',
            serviceLabel: `${item.name ?? 'Combo'} (package)`,
            hasSelectedCurrencyPrice: true,
            prices: []
        };
        return option;
    }

    // Mirrors the backend PricingService chain
    // explicit per-currency ServicePrice -> USD price/base converted via the effective FX rate.
    // The backend re-resolves authoritatively at checkout (incl. special prices).
    private withSelectedCurrencyPrice(item: ServiceOption): ServiceOption {
        const currency = (this.currency || 'USD').toUpperCase();
        const explicit = item.prices.find((p) => String(p.currencyCode).toUpperCase() === currency && p.isActive !== false);
        const usdExplicit = item.prices.find((p) => String(p.currencyCode).toUpperCase() === 'USD' && p.isActive !== false);
        const usdBase = usdExplicit ? Number(usdExplicit.unitPrice) : Number(item.basePriceUsd);

        let unitPrice = 0;
        let hasSelectedCurrencyPrice = false;
        let converted = false;

        if (explicit) {
            unitPrice = Number(explicit.unitPrice);
            hasSelectedCurrencyPrice = true;
        } else if (currency === 'USD' && usdBase > 0) {
            unitPrice = usdBase;
            hasSelectedCurrencyPrice = true;
        } else if (usdBase > 0) {
            const rate = this.ratesMap[currency];
            if (rate && rate > 0) {
                unitPrice = Math.round(usdBase * rate * 100) / 100;
                hasSelectedCurrencyPrice = true;
                converted = true;
            }
        }

        const serviceLabel = hasSelectedCurrencyPrice
            ? `${item.serviceName} - ${this.money(unitPrice, currency)}${converted ? ' (converted)' : ''}`
            : `${item.serviceName} - missing ${currency} price`;

        return { ...item, unitPrice, currency, hasSelectedCurrencyPrice, serviceLabel };
    }

    private loadExchangeRatesMap(): void {
        const today = new Date();
        const date = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
        this.api.get<any>(`/api/exchange-rates/effective?date=${date}`).subscribe({
            next: (rates) => {
                const map: Record<string, number> = { USD: 1 };
                (Array.isArray(rates) ? rates : []).forEach((r: any) => {
                    if (r.currencyCode && Number(r.rate) > 0) map[String(r.currencyCode).toUpperCase()] = Number(r.rate);
                });
                this.ratesMap = map;
                // Re-price any already-loaded services now that rates are available.
                this.services.update((rows) => rows.map((item) => this.withSelectedCurrencyPrice(item)));
            },
            error: () => { this.ratesMap = { USD: 1 }; }
        });
    }

    previewVoucherNumber(): string {
        const max = this.bookings()
            .map((booking) => String(booking.voucherCode ?? '').replace(/\D/g, ''))
            .map((value) => Number(value))
            .filter((value) => Number.isFinite(value))
            .reduce((highest, value) => Math.max(highest, value), 0);
        return String(max + 1).padStart(5, '0');
    }

    private validateStep(step: number): boolean {
        this.status.set('');
        if (step === 0) {
            const hasName = !!(this.client.firstName || this.client.surname);
            const hasContact = !!(this.client.email || this.client.phoneNumber);
            if (!this.agencyVoucherRef.trim()) {
                this.status.set('Agency voucher reference is required.');
                return false;
            }
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
