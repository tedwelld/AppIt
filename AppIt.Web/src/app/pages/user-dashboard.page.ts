import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
              <option *ngFor="let room of accommodations()" [value]="room.id">
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

            <button class="btn-base btn-primary" type="submit">Reserve & Generate Invoice</button>
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
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let res of reservations()">
                  <td>{{ res.reference }}</td>
                  <td>{{ res.voucherCode }}</td>
                  <td>{{ res.status }}</td>
                  <td>{{ formatMoney(res.totalAmount, res.currency) }}</td>
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
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let invoice of invoices()">
                  <td>{{ invoice.id }}</td>
                  <td>{{ invoice.status }}</td>
                  <td>{{ formatMoney(invoice.totalAmount, invoice.currency) }}</td>
                  <td>{{ invoice.issuedAt | date: 'mediumDate' }}</td>
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
      gap: 0.9rem;
      min-height: 0;
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

    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 0.8rem; }
    .panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      padding: 0.8rem;
      background: #fff;
      display: grid;
      gap: 0.6rem;
      min-height: 0;
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

    .booking-form { display: grid; gap: 0.45rem; }

    .table-wrap { border: 1px solid #e1e8f1; border-radius: 0.8rem; overflow: auto; }
    table { width: 100%; border-collapse: collapse; font-size: 0.82rem; }
    th, td { padding: 0.5rem 0.55rem; border-bottom: 1px solid #edf1f6; text-align: left; }
    th { background: #0f4c5c; color: #fff; position: sticky; top: 0; }
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
    roomId: '',
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

    const reservation: Reservation = {
      id: `RES-${Date.now()}`,
      userId: this.profile().id,
      reference,
      voucherCode,
      currency: this.preferredCurrency,
      totalAmount: total,
      status: 'Pending',
      createdAt: new Date().toISOString()
    };

    this.api.post('/api/reservations', reservation).pipe(catchError(() => of(reservation))).subscribe(() => {
      this.reservations.update((list) => [reservation, ...list]);
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

      this.status.set(`Activities booked. Voucher ${voucherCode} generated.`);
      this.selectedActivities.set([]);
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

    const reservation: Reservation = {
      id: `RES-${Date.now()}`,
      userId: this.profile().id,
      reference,
      voucherCode,
      currency: this.preferredCurrency,
      totalAmount: total,
      status: 'Pending',
      createdAt: new Date().toISOString()
    };

    const invoice: Invoice = {
      id: `INV-${Date.now()}`,
      reservationId: reservation.id,
      totalAmount: total,
      currency: this.preferredCurrency,
      status: 'Pending',
      issuedAt: new Date().toISOString()
    };

    this.api.post('/api/reservations', reservation).pipe(catchError(() => of(reservation))).subscribe(() => {
      this.reservations.update((list) => [reservation, ...list]);
    });
    this.api.post('/api/invoices', invoice).pipe(catchError(() => of(invoice))).subscribe(() => {
      this.invoices.update((list) => [invoice, ...list]);
    });
    this.api.post('/api/vouchers', { code: voucherCode, reference, type: 'Accommodation' }).pipe(catchError(() => of({}))).subscribe();
    this.vouchers.update((list) => [
      ...list,
      { id: `VCH-${Date.now()}`, code: voucherCode, reference, type: 'Accommodation', createdAt: new Date().toISOString() }
    ]);

    this.status.set(`Reservation created. Invoice ${invoice.id} is Pending.`);
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
    ).subscribe((rows) => this.products.set(rows as Product[]));
  }

  private loadAccommodations(): void {
    this.api.list('/api/accommodations').pipe(
      catchError(() => of(this.sampleAccommodations()))
    ).subscribe((rows) => {
      const data = rows as Accommodation[];
      this.accommodations.set(data);
      if (!this.booking.roomId && data.length) {
        this.booking.roomId = data[0].id;
      }
    });
  }

  private loadActivities(): void {
    this.api.list('/api/activities').pipe(
      catchError(() => of(this.sampleActivities()))
    ).subscribe((rows) => this.activities.set(rows as Activity[]));
  }

  private loadReservations(): void {
    this.api.list('/api/reservations/mine').pipe(
      catchError(() => of([]))
    ).subscribe((rows) => this.reservations.set(rows as Reservation[]));
  }

  private loadInvoices(): void {
    this.api.list('/api/invoices/mine').pipe(
      catchError(() => of([]))
    ).subscribe((rows) => this.invoices.set(rows as Invoice[]));
  }

  private loadVouchers(): void {
    this.api.list('/api/vouchers/mine').pipe(
      catchError(() => of([]))
    ).subscribe((rows) => this.vouchers.set(rows as Voucher[]));
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
      { id: 'prd-bungee', name: 'Bungee', description: 'Leap from the gorge with expert guides.', category: 'Adventure', basePriceUsd: 120 },
      { id: 'prd-swing', name: 'Swing', description: 'A giant swing over the river.', category: 'Adventure', basePriceUsd: 95 },
      { id: 'prd-bridge', name: 'Bridge Tours', description: 'Guided bridge walk and history.', category: 'Tours', basePriceUsd: 55 },
      { id: 'prd-falls', name: 'View of the Falls', description: 'Sunrise viewpoints and photo stops.', category: 'Scenic', basePriceUsd: 35 },
      { id: 'prd-boat', name: 'Boat Cruise', description: 'Sunset cruise with refreshments.', category: 'Water', basePriceUsd: 80 }
    ];
  }

  private sampleAccommodations(): Accommodation[] {
    return [
      { id: 'acc-single', type: 'Single', description: 'Cozy single room with balcony.', capacity: 1, basePriceUsd: 75 },
      { id: 'acc-double', type: 'Double', description: 'Double room with panoramic views.', capacity: 2, basePriceUsd: 120 },
      { id: 'acc-express', type: 'Express', description: 'Fast check-in business suite.', capacity: 2, basePriceUsd: 150 },
      { id: 'acc-standard', type: 'Standard', description: 'Classic comfort room.', capacity: 2, basePriceUsd: 95 }
    ];
  }

  private sampleActivities(): Activity[] {
    return [
      { id: 'act-bungee', name: 'Bungee', description: 'High-adrenaline jump experience.', basePriceUsd: 120 },
      { id: 'act-swing', name: 'Gorge Swing', description: 'Swing over the misty gorge.', basePriceUsd: 90 },
      { id: 'act-bridge', name: 'Bridge Tour', description: 'Architecture and history tour.', basePriceUsd: 55 },
      { id: 'act-falls', name: 'Falls View', description: 'Guided falls vista walk.', basePriceUsd: 35 },
      { id: 'act-boat', name: 'Boat Cruise', description: 'Sunset cruise and wildlife.', basePriceUsd: 80 }
    ];
  }
}
