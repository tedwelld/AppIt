import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { jsPDF } from 'jspdf';
import { catchError, of } from 'rxjs';
import { ApiService } from '../api.service';
import { AuthService } from '../auth.service';
import { Accommodation, Activity, CurrencyCode, Invoice, Product, Reservation, Voucher } from '../models';

@Component({
  selector: 'app-user-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="user-dashboard">
      <header class="hero">
        <div>
          <p class="eyebrow">Welcome back</p>
          <h1>{{ displayName() }}</h1>
          <p class="sub">Your reservations, vouchers, and next adventure in one place.</p>
        </div>
        <div class="profile-card">
          <div class="avatar">
            <span *ngIf="!profile().avatarUrl">{{ initials() }}</span>
            <img *ngIf="profile().avatarUrl" [src]="profile().avatarUrl" [alt]="displayName()" />
          </div>
          <div>
            <p class="label">Currency</p>
            <select class="app-select" [(ngModel)]="preferredCurrency" (ngModelChange)="updateCurrency($event)">
              <option *ngFor="let code of currencies" [value]="code">{{ code }}</option>
            </select>
            <p class="small">Email: {{ profile().email }}</p>
          </div>
        </div>
      </header>

      <section class="stats">
        <article class="stat-card">
          <p class="label">Reservations</p>
          <p class="value">{{ reservations().length }}</p>
          <p class="sub">Active bookings this season.</p>
        </article>
        <article class="stat-card">
          <p class="label">Vouchers</p>
          <p class="value">{{ vouchers().length }}</p>
          <p class="sub">Instant references generated.</p>
        </article>
        <article class="stat-card">
          <p class="label">Invoices</p>
          <p class="value">{{ invoices().length }}</p>
          <p class="sub">Payment status synced.</p>
        </article>
      </section>

      <div class="grid">
        <article class="panel">
          <h2>Available Products</h2>
          <div class="item-grid">
            <div class="item-card" *ngFor="let product of products()">
              <h3>{{ product.name }}</h3>
              <p>{{ product.description }}</p>
              <p class="price">{{ formatMoney(convertPrice(product.basePriceUsd), preferredCurrency) }}</p>
              <button class="btn-base btn-secondary" (click)="selectActivity(product)">Add to Plan</button>
            </div>
          </div>
        </article>

        <article class="panel">
          <h2>Accommodation Booking</h2>
          <form (ngSubmit)="bookAccommodation()" class="booking-form">
            <label>Room Type</label>
            <select class="app-select" [(ngModel)]="booking.roomId" name="roomId">
              <option *ngFor="let room of accommodations()" [ngValue]="room.id">
                {{ room.type }} (Sleeps {{ room.capacity }}) - {{ formatMoney(convertPrice(room.basePriceUsd), preferredCurrency) }}/night
              </option>
            </select>

            <label>Check In</label>
            <input class="app-input" type="date" [(ngModel)]="booking.checkIn" name="checkIn" />

            <label>Nights</label>
            <input class="app-input" type="number" min="1" [(ngModel)]="booking.nights" name="nights" />

            <label>Guests</label>
            <input class="app-input" type="number" min="1" [(ngModel)]="booking.guests" name="guests" />

            <label>Payment Method</label>
            <select class="app-select" [(ngModel)]="booking.paymentMethod" name="paymentMethod">
              <option *ngFor="let method of paymentMethods" [value]="method">{{ method }}</option>
            </select>

            <button class="btn-base btn-primary" type="submit">Reserve & Generate Invoice PDF</button>
          </form>
          <p class="status" *ngIf="status()">{{ status() }}</p>
        </article>
      </div>

      <div class="grid">
        <article class="panel">
          <h2>Selected Activities</h2>
          <div class="item-grid">
            <div class="item-card" *ngFor="let activity of selectedActivities()">
              <h3>{{ activity.name }}</h3>
              <p>{{ activity.description }}</p>
              <p class="price">{{ formatMoney(convertPrice(activity.basePriceUsd), preferredCurrency) }}</p>
              <button class="btn-base btn-secondary" (click)="removeActivity(activity)">Remove</button>
            </div>
          </div>
          <div class="summary" *ngIf="selectedActivities().length">
            <p>Total Activities: {{ selectedActivities().length }}</p>
            <p class="price">{{ formatMoney(totalActivities(), preferredCurrency) }}</p>
            <button class="btn-base btn-primary" (click)="bookActivities()">Book Activities</button>
          </div>
        </article>

        <article class="panel">
          <h2>My Reservations</h2>
          <div class="table-wrap">
            <table *ngIf="reservations().length; else noReservations">
              <thead>
                <tr>
                  <th>Reference</th>
                  <th>Voucher</th>
                  <th>Status</th>
                  <th>Total</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let res of reservations()">
                  <td>{{ res.reference }}</td>
                  <td>{{ res.voucherCode }}</td>
                  <td>{{ res.status }}</td>
                  <td>{{ formatMoney(res.totalAmount, res.currency) }}</td>
                  <td class="actions">
                    <button class="btn-base btn-secondary" (click)="deleteReservation(res)" [disabled]="!canDeleteReservation(res)">
                      Delete
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
            <ng-template #noReservations>
              <p class="empty">No reservations yet.</p>
            </ng-template>
          </div>
        </article>
      </div>

      <div class="grid">
        <article class="panel">
          <h2>Invoices & Payments</h2>
          <div class="table-wrap">
            <table *ngIf="invoices().length; else noInvoices">
              <thead>
                <tr>
                  <th>Invoice</th>
                  <th>Status</th>
                  <th>Amount</th>
                  <th>Issued</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let invoice of invoices()">
                  <td>{{ invoice.id }}</td>
                  <td>{{ invoice.status }}</td>
                  <td>{{ formatMoney(invoice.totalAmount, invoice.currency) }}</td>
                  <td>{{ invoice.issuedAt | date: 'mediumDate' }}</td>
                  <td class="actions">
                    <button class="btn-base btn-secondary" (click)="downloadInvoicePdf(invoice)">PDF</button>
                    <button class="btn-base btn-primary" (click)="payInvoice(invoice)">Pay</button>
                  </td>
                </tr>
              </tbody>
            </table>
            <ng-template #noInvoices>
              <p class="empty">No invoices issued.</p>
            </ng-template>
          </div>
        </article>

        <article class="panel">
          <h2>My Vouchers</h2>
          <div class="voucher-grid">
            <div class="voucher" *ngFor="let voucher of vouchers()">
              <h3>{{ voucher.code }}</h3>
              <p>{{ voucher.reference }}</p>
              <span>{{ voucher.type }}</span>
            </div>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: `
    .user-dashboard {
      display: grid;
      gap: 0.7rem;
      min-height: 0;
      height: 100%;
      overflow: auto;
    }
    .hero {
      border-radius: 1.2rem;
      padding: 1rem;
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      background: linear-gradient(135deg, #ffffff, #f3fbff);
      border: 1px solid #dbe3ef;
    }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.12em; font-size: 0.7rem; color: #0f4c5c; font-weight: 700; }
    .hero h1 { margin: 0.2rem 0; font-size: 1.5rem; }
    .sub { margin: 0; color: #5b6f85; font-size: 0.9rem; }
    .profile-card {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 0.6rem;
      align-items: center;
      border: 1px solid #d9e4f1;
      background: #fff;
      border-radius: 0.9rem;
      padding: 0.7rem;
      min-width: 240px;
    }
    .avatar {
      width: 2.7rem;
      height: 2.7rem;
      border-radius: 0.8rem;
      background: linear-gradient(145deg, #0f4c5c, #2f8c88);
      color: #fff;
      display: grid;
      place-items: center;
      font-weight: 800;
      overflow: hidden;
    }
    .avatar img { width: 100%; height: 100%; object-fit: cover; }
    .label { margin: 0; font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.08em; color: #607287; }
    .small { margin: 0.25rem 0 0; font-size: 0.75rem; color: #5b6f85; }

    .stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 0.7rem;
    }
    .stat-card {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      padding: 0.8rem;
      background: #fff;
      box-shadow: 0 10px 18px rgba(15, 43, 74, 0.08);
    }
    .stat-card .value { margin: 0.2rem 0; font-size: 1.3rem; font-weight: 800; color: #0f4c5c; }
    .stat-card .sub { margin: 0; font-size: 0.82rem; }

    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 0.7rem; align-items: start; }
    .panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      padding: 0.8rem;
      background: #fff;
      display: grid;
      gap: 0.6rem;
      min-height: 0;
      max-height: 420px;
      overflow: auto;
    }
    .panel h2 { margin: 0; font-size: 1.05rem; color: #14334f; }
    .item-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 0.6rem; }
    .item-card {
      border: 1px solid #e1e8f1;
      border-radius: 0.9rem;
      padding: 0.6rem;
      background: linear-gradient(160deg, #ffffff, #f7fbff);
      display: grid;
      gap: 0.4rem;
    }
    .item-card h3 { margin: 0; font-size: 0.95rem; }
    .item-card p { margin: 0; font-size: 0.82rem; color: #5b6f85; }
    .price { margin: 0; font-weight: 800; color: #0f4c5c; }

    .booking-form { display: grid; gap: 0.45rem; grid-template-columns: repeat(2, minmax(0, 1fr)); align-items: end; }
    .booking-form button, .booking-form .status { grid-column: 1 / -1; }

    .table-wrap { border: 1px solid #e1e8f1; border-radius: 0.8rem; overflow: auto; }
    table { width: 100%; min-width: 760px; border-collapse: collapse; font-size: 0.82rem; }
    th, td { padding: 0.5rem 0.55rem; border-bottom: 1px solid #edf1f6; text-align: left; }
    th { background: #0f4c5c; color: #fff; position: sticky; top: 0; }
    .actions { white-space: nowrap; }
    .actions button { margin-right: 0.35rem; }
    .empty { margin: 0; padding: 0.8rem; color: #6d8096; }

    .voucher-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 0.6rem; }
    .voucher {
      border: 1px dashed #b7c7da;
      border-radius: 0.8rem;
      padding: 0.6rem;
      background: #f6fbff;
      display: grid;
      gap: 0.35rem;
    }
    .voucher h3 { margin: 0; font-size: 0.88rem; }
    .voucher span { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; color: #0f4c5c; }
    .summary { display: flex; justify-content: space-between; align-items: center; gap: 0.6rem; flex-wrap: wrap; }
    .status { margin: 0; font-size: 0.82rem; color: #0f4c5c; background: #eaf7f5; border: 1px solid #cce8e3; border-radius: 0.7rem; padding: 0.4rem 0.6rem; }

    @media (max-width: 900px) {
      .hero { flex-direction: column; }
      .booking-form { grid-template-columns: 1fr; }
    }
  `
})
export class UserDashboardPageComponent {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  private readonly fallbackProfile = {
    id: 'guest',
    role: 'regular' as const,
    firstName: 'Guest',
    lastName: 'User',
    email: 'guest@appit.com',
    preferredCurrency: 'USD' as const,
    phone: '',
    avatarUrl: ''
  };

  readonly profile = signal(this.auth.user() ?? this.fallbackProfile);
  readonly displayName = computed(() => `${this.profile().firstName} ${this.profile().lastName}`.trim());
  readonly initials = computed(
    () => `${this.profile().firstName?.[0] ?? ''}${this.profile().lastName?.[0] ?? ''}`.toUpperCase() || 'GU'
  );

  readonly products = signal<Product[]>([]);
  readonly accommodations = signal<Accommodation[]>([]);
  readonly activities = signal<Activity[]>([]);
  readonly selectedActivities = signal<Activity[]>([]);
  readonly reservations = signal<Reservation[]>([]);
  readonly invoices = signal<Invoice[]>([]);
  readonly vouchers = signal<Voucher[]>([]);
  readonly status = signal('');

  readonly currencies: CurrencyCode[] = ['USD', 'ZAR', 'GBP'];
  preferredCurrency: CurrencyCode = this.profile().preferredCurrency ?? 'USD';

  booking = {
    roomId: null as number | null,
    checkIn: '',
    nights: 1,
    guests: 1,
    paymentMethod: 'Mastercard'
  };

  readonly paymentMethods = ['Mastercard', 'PayPal', 'CashApp', 'EcoCash', 'Bank Transfer'];

  private readonly currencyRates: Record<CurrencyCode, number> = {
    USD: 1,
    ZAR: 18.5,
    GBP: 0.8
  };

  constructor() {
    this.loadProducts();
    this.loadAccommodations();
    this.loadActivities();
    this.loadReservations();
    this.loadInvoices();
    this.loadVouchers();
  }

  selectActivity(product: Product): void {
    const mapped: Activity = {
      id: product.id,
      name: product.name,
      description: product.description,
      basePriceUsd: product.basePriceUsd
    };
    this.selectedActivities.update((list) => [...list, mapped]);
  }

  removeActivity(activity: Activity): void {
    this.selectedActivities.update((list) => list.filter((item) => item !== activity));
  }

  totalActivities(): number {
    const totalUsd = this.selectedActivities().reduce((sum, item) => sum + item.basePriceUsd, 0);
    return this.convertPrice(totalUsd);
  }

  bookActivities(): void {
    if (!this.selectedActivities().length) {
      return;
    }

    const reference = this.buildReference('ACT');
    const voucherCode = this.buildVoucher('Activity');
    const total = this.totalActivities();

    this.api.post('/api/reservations', {
      reference,
      voucherCode,
      currency: this.preferredCurrency,
      totalAmount: total,
      status: 'Pending',
      customerEmail: this.profile().email
    }).pipe(
      catchError(() => of(null))
    ).subscribe((row) => {
      if (!row) {
        this.status.set('Failed to book activities.');
        return;
      }

      const reservation = this.normalizeReservation(row);
      this.reservations.update((list) => [reservation, ...list]);
      this.api.post('/api/invoices', {
        reservationId: reservation.id,
        totalAmount: total,
        currency: this.preferredCurrency,
        status: 'Pending'
      }).subscribe({
        next: (invoiceRow) => {
          const invoice = this.normalizeInvoice(invoiceRow);
          this.invoices.update((list) => [invoice, ...list]);
          this.downloadInvoicePdf(invoice);

          this.api.post('/api/vouchers', { code: voucherCode, reference, type: 'Activity' }).pipe(catchError(() => of({}))).subscribe();
          this.vouchers.update((list) => [
            ...list,
            {
              id: `VCH-${Date.now()}`,
              code: voucherCode,
              reference,
              type: 'Activity',
              createdAt: new Date().toISOString()
            }
          ]);

          this.status.set(`Activities booked. Invoice ${invoice.id} generated and voucher ${voucherCode} created.`);
          this.selectedActivities.set([]);

          if (window.confirm(`Invoice ${invoice.id} is ready. Pay now with ${this.booking.paymentMethod}?`)) {
            this.payInvoice(invoice);
          }
        },
        error: (err) => this.status.set(this.describeError(err))
      });
    });
  }

  bookAccommodation(): void {
    const room = this.accommodations().find((item) => item.id === this.booking.roomId) ?? this.accommodations()[0];
    if (!room) {
      this.status.set('Select an accommodation type.');
      return;
    }

    const total = this.convertPrice(room.basePriceUsd) * Math.max(1, this.booking.nights || 1);
    const reference = this.buildReference('ACC');
    const voucherCode = this.buildVoucher('Accommodation');

    this.api.post('/api/reservations', {
      reference,
      voucherCode,
      currency: this.preferredCurrency,
      totalAmount: total,
      status: 'Pending',
      customerEmail: this.profile().email
    }).subscribe({
      next: (reservationRow) => {
        const reservation = this.normalizeReservation(reservationRow);
        this.reservations.update((list) => [reservation, ...list]);

        this.api.post('/api/invoices', {
          reservationId: reservation.id,
          totalAmount: total,
          currency: this.preferredCurrency,
          status: 'Pending'
        }).subscribe({
          next: (invoiceRow) => {
            const invoice = this.normalizeInvoice(invoiceRow);
            this.invoices.update((list) => [invoice, ...list]);
            this.downloadInvoicePdf(invoice);

            this.api.post('/api/vouchers', { code: voucherCode, reference, type: 'Accommodation' }).pipe(
              catchError(() => of({}))
            ).subscribe();
            this.vouchers.update((list) => [
              ...list,
              {
                id: `VCH-${Date.now()}`,
                code: voucherCode,
                reference,
                type: 'Accommodation',
                createdAt: new Date().toISOString()
              }
            ]);

            this.status.set(`Invoice ${invoice.id} generated and PDF downloaded.`);

            if (window.confirm(`Invoice ${invoice.id} is ready. Pay now with ${this.booking.paymentMethod}?`)) {
              this.payInvoice(invoice);
            }
          },
          error: (err) => this.status.set(this.describeError(err))
        });
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  payInvoice(invoice: Invoice): void {
    this.api.post('/api/payments/process', {
      invoiceId: invoice.id,
      method: this.booking.paymentMethod,
      amount: invoice.totalAmount,
      currencyCode: invoice.currency,
      returnUrl: `${window.location.origin}/user`,
      cancelUrl: `${window.location.origin}/user`
    }).subscribe({
      next: (res) => {
        const status = String(res?.status ?? 'Pending');
        const ref = String(res?.transactionReference ?? '');
        const provider = String(res?.provider ?? this.booking.paymentMethod);
        const redirectUrl = res?.redirectUrl as string | null | undefined;

        if (status.toLowerCase() === 'paid') {
          this.invoices.update((rows) => rows.map((item) => item.id === invoice.id ? { ...item, status: 'Paid' } : item));
        }

        this.status.set(`Payment ${status} via ${provider}. Ref: ${ref}`);
        this.loadInvoices();

        if (redirectUrl) {
          window.open(redirectUrl, '_blank', 'noopener,noreferrer');
        }
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  canDeleteReservation(reservation: Reservation): boolean {
    const relatedInvoices = this.invoices().filter((invoice) => invoice.reservationId === reservation.id);
    return !relatedInvoices.some((invoice) => invoice.status.toLowerCase() === 'paid');
  }

  deleteReservation(reservation: Reservation): void {
    if (!this.canDeleteReservation(reservation)) {
      this.status.set(`Reservation ${reservation.reference} cannot be deleted because it has already been paid.`);
      return;
    }

    if (!window.confirm(`Delete reservation ${reservation.reference}?`)) {
      return;
    }

    this.api.delete(`/api/reservations/${reservation.id}`).subscribe({
      next: () => {
        this.reservations.update((rows) => rows.filter((row) => row.id !== reservation.id));
        this.invoices.update((rows) => rows.filter((row) => row.reservationId !== reservation.id));
        this.status.set(`Reservation ${reservation.reference} deleted.`);
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  downloadInvoicePdf(invoice: Invoice): void {
    const doc = new jsPDF();
    doc.setFillColor(18, 46, 83);
    doc.rect(0, 0, 210, 28, 'F');
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(16);
    doc.text('AppIt Invoice', 14, 17);

    doc.setTextColor(20, 36, 54);
    doc.setFontSize(12);
    doc.text(`Invoice ID: ${invoice.id}`, 14, 42);
    doc.text(`Status: ${invoice.status}`, 14, 50);
    doc.text(`Currency: ${invoice.currency}`, 14, 58);
    doc.text(`Total: ${this.formatMoney(invoice.totalAmount, invoice.currency)}`, 14, 66);
    doc.text(`Issued: ${new Date(invoice.issuedAt).toLocaleString()}`, 14, 74);
    doc.text(`Guest: ${this.profile().firstName} ${this.profile().lastName}`, 14, 82);
    doc.text(`Email: ${this.profile().email}`, 14, 90);

    doc.setFontSize(10);
    doc.text('Thank you for booking with AppIt.', 14, 110);
    doc.save(`invoice-${invoice.id}.pdf`);
  }

  convertPrice(baseUsd: number): number {
    return Number((baseUsd * this.currencyRates[this.preferredCurrency]).toFixed(2));
  }

  updateCurrency(code: CurrencyCode): void {
    this.preferredCurrency = code;
    this.auth.updateProfile({ preferredCurrency: code });
    this.profile.update((current) => ({ ...current, preferredCurrency: code }));
  }

  formatMoney(amount: number, currency: CurrencyCode): string {
    const symbol = currency === 'USD' ? '$' : currency === 'ZAR' ? 'R' : '£';
    return `${symbol}${amount.toFixed(2)} ${currency}`;
  }

  private loadProducts(): void {
    this.api.list('/api/products').pipe(
      catchError(() => of(this.sampleProducts()))
    ).subscribe((rows) => this.products.set((rows as any[]).map((r) => this.normalizeProduct(r))));
  }

  private loadAccommodations(): void {
    this.api.list('/api/accommodations').pipe(
      catchError(() => of(this.sampleAccommodations()))
    ).subscribe((rows) => {
      const data = (rows as any[]).map((r) => this.normalizeAccommodation(r));
      this.accommodations.set(data);
      if (this.booking.roomId == null && data.length) {
        this.booking.roomId = data[0].id;
      }
    });
  }

  private loadActivities(): void {
    this.api.list('/api/activities').pipe(
      catchError(() => of(this.sampleActivities()))
    ).subscribe((rows) => this.activities.set((rows as any[]).map((r) => this.normalizeActivity(r))));
  }

  private loadReservations(): void {
    const email = encodeURIComponent(this.profile().email ?? '');
    this.api.list(`/api/reservations/mine?email=${email}`).pipe(
      catchError(() => of([]))
    ).subscribe((rows) => this.reservations.set((rows as any[]).map((r) => this.normalizeReservation(r))));
  }

  private loadInvoices(): void {
    this.api.list('/api/invoices/mine').pipe(
      catchError(() => of([]))
    ).subscribe((rows) => this.invoices.set((rows as any[]).map((r) => this.normalizeInvoice(r))));
  }

  private loadVouchers(): void {
    this.api.list('/api/vouchers/mine').pipe(
      catchError(() => of([]))
    ).subscribe((rows) => this.vouchers.set(rows as Voucher[]));
  }

  private normalizeProduct(row: any): Product {
    return {
      id: Number(row?.productId ?? row?.id ?? 0),
      name: String(row?.name ?? ''),
      description: row?.description ? String(row.description) : '',
      category: String(row?.category ?? 'Adventure'),
      basePriceUsd: Number(row?.basePriceUsd ?? 0)
    };
  }

  private normalizeAccommodation(row: any): Accommodation {
    return {
      id: Number(row?.id ?? 0),
      type: String(row?.type ?? 'Standard') as Accommodation['type'],
      description: row?.description ? String(row.description) : '',
      capacity: Number(row?.capacity ?? 1),
      basePriceUsd: Number(row?.basePriceUsd ?? 0)
    };
  }

  private normalizeActivity(row: any): Activity {
    return {
      id: Number(row?.id ?? 0),
      name: String(row?.name ?? ''),
      description: row?.description ? String(row.description) : '',
      basePriceUsd: Number(row?.basePriceUsd ?? 0)
    };
  }

  private normalizeReservation(row: any): Reservation {
    return {
      id: Number(row?.reservationId ?? row?.id ?? 0),
      reference: String(row?.reference ?? ''),
      voucherCode: String(row?.voucherCode ?? ''),
      currency: String(row?.currency ?? 'USD') as CurrencyCode,
      totalAmount: Number(row?.totalAmount ?? 0),
      status: String(row?.status ?? 'Pending') as Reservation['status'],
      createdAt: String(row?.createdAt ?? new Date().toISOString())
    };
  }

  private normalizeInvoice(row: any): Invoice {
    return {
      id: Number(row?.id ?? 0),
      reservationId: Number(row?.reservationId ?? 0),
      totalAmount: Number(row?.totalAmount ?? 0),
      currency: String(row?.currency ?? 'USD') as CurrencyCode,
      status: String(row?.status ?? 'Pending') as Invoice['status'],
      issuedAt: String(row?.issuedAt ?? new Date().toISOString())
    };
  }

  private describeError(err: any): string {
    const fromApi = err?.error?.message ?? err?.error?.title ?? err?.message;
    return fromApi ? `Request failed: ${fromApi}` : 'Request failed';
  }

  private buildReference(prefix: string): string {
    const now = new Date();
    const stamp = `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
    const rand = Math.floor(1000 + Math.random() * 9000);
    return `APP-${prefix}-${stamp}-${rand}`;
  }

  private buildVoucher(type: Voucher['type']): string {
    const rand = Math.floor(100000 + Math.random() * 900000);
    return `VCH-${type.substring(0, 3).toUpperCase()}-${rand}`;
  }

  private sampleProducts(): Product[] {
    return [
      { id: 1, name: 'Bungee', description: 'Leap from the gorge with expert guides.', category: 'Adventure', basePriceUsd: 120 },
      { id: 2, name: 'Swing', description: 'A giant swing over the river.', category: 'Adventure', basePriceUsd: 95 },
      { id: 3, name: 'Bridge Tours', description: 'Guided bridge walk and history.', category: 'Tours', basePriceUsd: 55 },
      { id: 4, name: 'View of the Falls', description: 'Sunrise viewpoints and photo stops.', category: 'Scenic', basePriceUsd: 35 },
      { id: 5, name: 'Boat Cruise', description: 'Sunset cruise with refreshments.', category: 'Water', basePriceUsd: 80 }
    ];
  }

  private sampleAccommodations(): Accommodation[] {
    return [
      { id: 1, type: 'Single', description: 'Cozy single room with balcony.', capacity: 1, basePriceUsd: 75 },
      { id: 2, type: 'Double', description: 'Double room with panoramic views.', capacity: 2, basePriceUsd: 120 },
      { id: 3, type: 'Express', description: 'Fast check-in business suite.', capacity: 2, basePriceUsd: 150 },
      { id: 4, type: 'Standard', description: 'Classic comfort room.', capacity: 2, basePriceUsd: 95 }
    ];
  }

  private sampleActivities(): Activity[] {
    return [
      { id: 1, name: 'Bungee', description: 'High-adrenaline jump experience.', basePriceUsd: 120 },
      { id: 2, name: 'Gorge Swing', description: 'Swing over the misty gorge.', basePriceUsd: 90 },
      { id: 3, name: 'Bridge Tour', description: 'Architecture and history tour.', basePriceUsd: 55 },
      { id: 4, name: 'Falls View', description: 'Guided falls vista walk.', basePriceUsd: 35 },
      { id: 5, name: 'Boat Cruise', description: 'Sunset cruise and wildlife.', basePriceUsd: 80 }
    ];
  }
}
