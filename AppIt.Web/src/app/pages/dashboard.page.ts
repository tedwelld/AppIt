import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';
import { ApiService } from '../api.service';
import { AuthService, UserProfile } from '../auth.service';
import { RESOURCE_CONFIGS } from '../entity-config';

interface DashboardCard {
  key: string;
  icon: string;
  title: string;
  summary: string;
  count: number;
  countText: string;
  hasError: boolean;
  colorClass: string;
}

interface AdminStats {
  range: string;
  totalSales: number;
  totalBookings: number;
  totalEarnings: number;
  totalCustomers: number;
  trend: number[];
  trendLabels?: string[];
}

interface VerificationItem {
  invoiceId: number;
  reservationId: number;
  totalAmount: number;
  currency: string;
  invoiceStatus: string;
  issuedAt: string;
  hasPaymentRecord: boolean;
  hasPaidPayment: boolean;
  paymentRecordCount: number;
  lastPaymentStatus: string;
  lastPaymentAt?: string;
  isValidated: boolean;
}

interface VerificationSummary {
  granularity: string;
  windowStartUtc: string;
  windowEndUtc: string;
  generatedAtUtc: string;
  totalInvoices: number;
  paidInvoices: number;
  unpaidInvoices: number;
  validatedInvoices: number;
  mismatchedInvoices: number;
  missingPaymentRecords: number;
  items: VerificationItem[];
}

interface CalendarDay {
  date: string;
  dayNumber: number;
  inCurrentMonth: boolean;
  reservations: any[];
  invoices: any[];
  payments: any[];
  sales: number;
}

interface PieSegment {
  label: string;
  value: number;
  percent: number;
  color: string;
  dasharray: string;
  dashoffset: number;
}

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="dashboard">
      <header class="hero">
        <div>
          <p class="eyebrow">AppIt Admin Panel</p>
          <h1>Operations Overview</h1>
          <p class="sub">Sales, bookings, earnings, and customer growth at a glance.</p>
        </div>
        <div class="hero-actions">
          <label>
            <span>Range</span>
            <select class="app-select" [(ngModel)]="selectedRange" (ngModelChange)="loadStats()">
              <option value="daily">Daily</option>
              <option value="weekly">Weekly</option>
              <option value="monthly">Monthly</option>
              <option value="quarterly">Quarterly</option>
              <option value="yearly">Yearly</option>
            </select>
          </label>
          <button class="btn-base btn-secondary" (click)="refreshAll()">
            <span class="material-symbols-outlined">refresh</span>
            Refresh All
          </button>
        </div>
      </header>

      <p class="status-error" *ngIf="statsError()">{{ statsError() }}</p>
      <p class="status-error" *ngIf="verificationError()">{{ verificationError() }}</p>
      <p class="status-error" *ngIf="calendarError()">{{ calendarError() }}</p>

      <section class="stat-grid" *ngIf="stats() as s">
        <article class="stat">
          <p class="label">Sales</p>
          <p class="value">{{ s.totalSales | number }}</p>
          <p class="sub">All completed payments.</p>
        </article>
        <article class="stat">
          <p class="label">Bookings</p>
          <p class="value">{{ s.totalBookings | number }}</p>
          <p class="sub">Reservations created.</p>
        </article>
        <article class="stat">
          <p class="label">Total Earnings</p>
          <p class="value">$ {{ s.totalEarnings | number:'1.0-0' }}</p>
          <p class="sub">Gross revenue.</p>
        </article>
        <article class="stat">
          <p class="label">Customers</p>
          <p class="value">{{ s.totalCustomers | number }}</p>
          <p class="sub">Active accounts.</p>
        </article>
      </section>

      <section class="chart-panel" *ngIf="stats() as s">
        <div class="chart-header">
          <div>
            <p class="eyebrow">Distribution</p>
            <h2>Revenue vs Bookings</h2>
          </div>
          <span class="pill">Range: {{ s.range }}</span>
        </div>
        <div class="pie-grid">
          <article class="pie-card">
            <svg class="pie-chart" viewBox="0 0 42 42" role="img" aria-label="Revenue share pie chart">
              <circle class="pie-track" cx="21" cy="21" r="15.915"></circle>
              <circle
                *ngFor="let segment of trendPieSegments(s.trend)"
                class="pie-segment"
                cx="21"
                cy="21"
                r="15.915"
                [attr.stroke]="segment.color"
                [attr.stroke-dasharray]="segment.dasharray"
                [attr.stroke-dashoffset]="segment.dashoffset"
              >
                <title>{{ pieTooltip(segment) }}</title>
              </circle>
            </svg>
            <div>
              <p class="sub">Revenue share by time window</p>
              <ul class="pie-legend">
                <li *ngFor="let segment of trendPieSegments(s.trend); let i = index">
                  <span class="legend-label">
                    <span class="legend-index" [style.background]="segment.color">{{ i + 1 }}</span>
                    <span class="legend-name">{{ segment.label }}</span>
                  </span>
                  <span class="legend-value">{{ segment.percent | number:'1.0-1' }}%</span>
                </li>
              </ul>
            </div>
          </article>

          <article class="pie-card">
            <svg class="pie-chart" viewBox="0 0 42 42" role="img" aria-label="Bookings share pie chart">
              <circle class="pie-track" cx="21" cy="21" r="15.915"></circle>
              <circle
                *ngFor="let segment of bookingPieSegments(s.trend)"
                class="pie-segment"
                cx="21"
                cy="21"
                r="15.915"
                [attr.stroke]="segment.color"
                [attr.stroke-dasharray]="segment.dasharray"
                [attr.stroke-dashoffset]="segment.dashoffset"
              >
                <title>{{ pieTooltip(segment) }}</title>
              </circle>
            </svg>
            <div>
              <p class="sub">Booking share by time window</p>
              <ul class="pie-legend">
                <li *ngFor="let segment of bookingPieSegments(s.trend); let i = index">
                  <span class="legend-label">
                    <span class="legend-index" [style.background]="segment.color">{{ i + 1 }}</span>
                    <span class="legend-name">{{ segment.label }}</span>
                  </span>
                  <span class="legend-value">{{ segment.percent | number:'1.0-1' }}%</span>
                </li>
              </ul>
            </div>
          </article>
        </div>
      </section>

      <section class="verification-panel">
        <div class="chart-header">
          <div>
            <p class="eyebrow">Verification</p>
            <h2>Invoice Payment Validation</h2>
          </div>
          <button class="btn-base btn-secondary" (click)="loadVerification()">
            <span class="material-symbols-outlined">published_with_changes</span>
            Regenerate
          </button>
        </div>

        <div class="verify-controls">
          <label>
            <span>Period</span>
            <select class="app-select" [(ngModel)]="verificationGranularity" (ngModelChange)="loadVerification()">
              <option value="moment">Moment</option>
              <option value="day">Day</option>
              <option value="month">Month</option>
              <option value="year">Year</option>
            </select>
          </label>
          <label *ngIf="verificationGranularity === 'moment'">
            <span>Timestamp (UTC)</span>
            <input class="app-input" type="datetime-local" [(ngModel)]="verificationAtMoment" (change)="loadVerification()" />
          </label>
          <label *ngIf="verificationGranularity === 'day'">
            <span>Day (UTC)</span>
            <input class="app-input" type="date" [(ngModel)]="verificationAtDay" (change)="loadVerification()" />
          </label>
          <label *ngIf="verificationGranularity === 'month'">
            <span>Month (UTC)</span>
            <input class="app-input" type="month" [(ngModel)]="verificationAtMonth" (change)="loadVerification()" />
          </label>
          <label *ngIf="verificationGranularity === 'year'">
            <span>Year (UTC)</span>
            <input class="app-input" type="number" min="2000" max="2100" [(ngModel)]="verificationAtYear" (change)="loadVerification()" />
          </label>
        </div>

        <section class="stat-grid" *ngIf="verification() as v">
          <article class="stat">
            <p class="label">Invoices</p>
            <p class="value">{{ v.totalInvoices }}</p>
            <p class="sub">Window: {{ v.windowStartUtc | date:'short' }} to {{ v.windowEndUtc | date:'short' }}</p>
          </article>
          <article class="stat">
            <p class="label">Validated</p>
            <p class="value">{{ v.validatedInvoices }}</p>
            <p class="sub">Invoice status matches payment records.</p>
          </article>
          <article class="stat">
            <p class="label">Mismatched</p>
            <p class="value">{{ v.mismatchedInvoices }}</p>
            <p class="sub">Needs supervisor review.</p>
          </article>
          <article class="stat">
            <p class="label">Missing Payments</p>
            <p class="value">{{ v.missingPaymentRecords }}</p>
            <p class="sub">Invoices marked paid without payment logs.</p>
          </article>
        </section>

        <div class="filter-row">
          <label>
            <span>Search Verification Rows</span>
            <input
              class="app-input"
              [ngModel]="verificationSearch()"
              (ngModelChange)="verificationSearch.set($event)"
              placeholder="Invoice, reservation, status..."
            />
          </label>
        </div>

        <div class="table-wrap" *ngIf="verification() as v">
          <table *ngIf="filteredVerificationItems(v.items).length">
            <thead>
              <tr>
                <th>Invoice</th>
                <th>Reservation</th>
                <th>Invoice Status</th>
                <th>Payment Status</th>
                <th>Records</th>
                <th>Issued</th>
                <th>Validated</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of filteredVerificationItems(v.items)">
                <td>{{ item.invoiceId }}</td>
                <td>{{ item.reservationId }}</td>
                <td>{{ item.invoiceStatus }}</td>
                <td>{{ item.lastPaymentStatus }}</td>
                <td>{{ item.paymentRecordCount }}</td>
                <td>{{ item.issuedAt | date:'short' }}</td>
                <td>{{ item.isValidated ? 'Yes' : 'No' }}</td>
              </tr>
            </tbody>
          </table>
          <p class="sub" *ngIf="!filteredVerificationItems(v.items).length">No matching invoices in the selected window.</p>
        </div>
      </section>

      <section class="verification-panel">
        <div class="chart-header">
          <div>
            <p class="eyebrow">Calendar</p>
            <h2>Monthly Reservation Calendar</h2>
          </div>
          <label>
            <span>Month</span>
            <input class="app-input" type="month" [(ngModel)]="calendarMonth" (change)="loadCalendarData()" />
          </label>
        </div>
        <div class="calendar-grid">
          <article
            class="calendar-cell"
            *ngFor="let day of calendarDays()"
            [class.dim]="!day.inCurrentMonth"
            [class.active]="selectedDay()?.date === day.date"
            (click)="selectCalendarDay(day)"
          >
            <p class="day">{{ day.dayNumber }}</p>
            <p class="meta">Res: {{ day.reservations.length }}</p>
            <p class="meta">Sales: {{ day.sales | number:'1.0-2' }}</p>
          </article>
        </div>
        <div class="table-wrap" *ngIf="selectedDay() as day">
          <table class="calendar-detail-table">
            <thead>
              <tr>
                <th colspan="5">Details for {{ day.date }}</th>
              </tr>
              <tr>
                <th>Reservations</th>
                <th>Invoices</th>
                <th>Payments</th>
                <th>Paid Sales</th>
                <th>Top Refs</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>{{ day.reservations.length }}</td>
                <td>{{ day.invoices.length }}</td>
                <td>{{ day.payments.length }}</td>
                <td>{{ day.sales | number:'1.0-2' }}</td>
                <td>{{ dayReservationRefs(day) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="chart-panel">
        <div class="chart-header">
          <div>
            <p class="eyebrow">Statistics Board</p>
            <h2>All Dashboard Card Data</h2>
          </div>
        </div>
        <article class="pie-card">
          <svg class="pie-chart" viewBox="0 0 42 42" role="img" aria-label="Dashboard card distribution pie chart">
            <circle class="pie-track" cx="21" cy="21" r="15.915"></circle>
            <circle
              *ngFor="let segment of cardPieSegments()"
              class="pie-segment"
              cx="21"
              cy="21"
              r="15.915"
              [attr.stroke]="segment.color"
              [attr.stroke-dasharray]="segment.dasharray"
              [attr.stroke-dashoffset]="segment.dashoffset"
            >
              <title>{{ pieTooltip(segment) }}</title>
            </circle>
          </svg>
          <div>
            <p class="sub">Card record share by module</p>
            <ul class="pie-legend">
              <li *ngFor="let segment of cardPieSegments(); let i = index">
                <span class="legend-label">
                  <span class="legend-index" [style.background]="segment.color">{{ i + 1 }}</span>
                  <span class="legend-name">{{ segment.label }}</span>
                </span>
                <span class="legend-value">{{ segment.percent | number:'1.0-1' }}%</span>
              </li>
            </ul>
          </div>
        </article>
      </section>

      <section class="verification-panel">
        <div class="chart-header">
          <div>
            <p class="eyebrow">Users</p>
            <h2>Online & Pending Approval</h2>
          </div>
          <button class="btn-base btn-secondary" (click)="loadUserApprovalData()">Refresh</button>
        </div>
        <div class="stat-grid">
          <article class="stat">
            <p class="label">Online Users</p>
            <p class="value">{{ onlineUsers().length }}</p>
          </article>
          <article class="stat">
            <p class="label">Pending Approval</p>
            <p class="value">{{ pendingUsers().length }}</p>
          </article>
        </div>
        <div class="filter-row">
          <label>
            <span>Search Users</span>
            <input
              class="app-input"
              [ngModel]="userSearch()"
              (ngModelChange)="userSearch.set($event)"
              placeholder="Name or email..."
            />
          </label>
        </div>
        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let user of filteredOnlineUsers()">
                <td>{{ user.firstName }} {{ user.lastName }}</td>
                <td>{{ user.email }}</td>
                <td>Online</td>
              </tr>
              <tr *ngFor="let user of filteredPendingUsers()">
                <td>{{ user.firstName }} {{ user.lastName }}</td>
                <td>{{ user.email }}</td>
                <td>Pending Approval</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <div class="filter-row">
        <label>
          <span>Search Modules</span>
          <input
            class="app-input"
            [ngModel]="moduleSearch()"
            (ngModelChange)="moduleSearch.set($event)"
            placeholder="Module title or summary..."
          />
        </label>
      </div>

      <section class="cards">
        <a
          class="card"
          [ngClass]="card.colorClass"
          *ngFor="let card of filteredCards(); let i = index"
          [routerLink]="['/entities', card.key]"
        >
          <div class="card-head">
            <div class="icon-wrap">
              <span class="material-symbols-outlined">{{ card.icon }}</span>
            </div>
            <div>
              <p class="index">#{{ i + 1 }}</p>
              <h3>{{ card.title }}</h3>
            </div>
          </div>

          <p class="count" [class.error-count]="card.hasError">{{ card.countText }}</p>
          <p class="desc">{{ card.summary }}</p>

          <div class="meta-row">
            <span class="pill" *ngIf="!card.hasError">Records: {{ card.count }}</span>
            <span class="pill error-pill" *ngIf="card.hasError">Source unavailable</span>
            <span class="open-label">Open module</span>
          </div>
        </a>
      </section>
    </section>
  `,
  styles: `
    .dashboard { display: grid; gap: 0.7rem; height: 100%; min-height: 0; overflow: auto; align-content: start; }
    .hero {
      background: linear-gradient(145deg, #ffffff, #eef7ff);
      border: 1px solid #d9e4f1;
      color: #133553;
      box-shadow: 0 8px 18px rgba(16, 37, 66, 0.08);
      border-radius: 1rem;
      padding: 0.95rem;
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }
    .hero-actions { display: grid; gap: 0.4rem; justify-items: end; }
    .hero-actions label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.08em; color: #607287; display: grid; gap: 0.3rem; }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.08em; font-size: 0.72rem; color: #0f4c5c; font-weight: 700; }
    h1 { margin: 0.2rem 0; font-size: 1.35rem; }
    .sub { margin: 0; max-width: 36rem; color: #5b6f85; font-size: 0.86rem; }

    .stat-grid {
      display: grid;
      gap: 0.7rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    }
    .filter-row label {
      display: grid;
      gap: 0.3rem;
      font-size: 0.72rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #607287;
    }
    .stat {
      border: 1px solid #dde5ef;
      border-radius: 0.95rem;
      padding: 0.7rem;
      background: #fff;
      box-shadow: 0 8px 16px rgba(16, 37, 66, 0.08);
    }
    .stat .label { margin: 0; font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.08em; color: #607287; }
    .stat .value { margin: 0.25rem 0; font-size: 1.3rem; font-weight: 800; color: #0f4c5c; }
    .stat .sub { margin: 0; font-size: 0.78rem; color: #5b6f85; }

    .chart-panel {
      border: 1px solid #d9e4f1;
      border-radius: 1rem;
      padding: 0.85rem;
      background: linear-gradient(165deg, #ffffff, #f8fbff);
      display: grid;
      gap: 0.5rem;
    }
    .chart-header { display: flex; justify-content: space-between; align-items: center; }
    .chart-panel h2 { margin: 0; font-size: 1.05rem; }
    .pie-grid {
      display: grid;
      gap: 0.7rem;
      grid-template-columns: repeat(auto-fit, minmax(420px, 1fr));
    }
    .pie-card {
      border: 1px solid #e2eaf4;
      border-radius: 0.75rem;
      background: #fff;
      padding: 0.65rem;
      display: grid;
      gap: 0.6rem;
      grid-template-columns: 1fr;
      align-items: center;
      justify-items: center;
    }
    .pie-chart {
      width: 340px;
      height: 340px;
      background: #fff;
      border-radius: 50%;
    }
    .pie-track {
      fill: none;
      stroke: #e3ecf7;
      stroke-width: 3.8;
    }
    .pie-segment {
      fill: none;
      stroke-width: 3.8;
      transform: rotate(-90deg);
      transform-origin: 50% 50%;
      cursor: pointer;
      opacity: 0.92;
      transition: stroke-width 150ms ease, opacity 150ms ease, filter 150ms ease;
    }
    .pie-segment:hover {
      stroke-width: 4.9;
      opacity: 1;
      filter: brightness(1.08);
    }
    .pie-legend {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: 0.38rem 0.45rem;
      max-height: 220px;
      overflow: auto;
      width: 100%;
    }
    .pie-legend li {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.75rem;
      color: #4f647a;
      border: 1px solid #dde8f5;
      border-radius: 0.55rem;
      padding: 0.26rem 0.34rem;
      background: #f8fbff;
      min-width: 0;
    }
    .legend-label {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      min-width: 0;
    }
    .legend-index {
      width: 1.1rem;
      height: 1.1rem;
      border-radius: 999px;
      color: #fff;
      font-weight: 800;
      font-size: 0.66rem;
      display: inline-grid;
      place-items: center;
      flex: none;
    }
    .legend-name {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .legend-value {
      font-weight: 700;
      color: #14334f;
      white-space: nowrap;
    }
    .verification-panel {
      border: 1px solid #d9e4f1;
      border-radius: 1rem;
      padding: 0.85rem;
      background: linear-gradient(165deg, #ffffff, #f8fbff);
      display: grid;
      gap: 0.7rem;
      max-height: 460px;
      overflow: auto;
    }
    .verification-panel h2 { margin: 0; font-size: 1.05rem; }
    .verify-controls {
      display: grid;
      gap: 0.5rem;
      grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
    }
    .verify-controls label {
      display: grid;
      gap: 0.3rem;
      font-size: 0.72rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #607287;
    }
    .calendar-grid {
      display: grid;
      grid-template-columns: repeat(7, minmax(0, 1fr));
      gap: 0.4rem;
    }
    .calendar-cell {
      border: 1px solid #d9e4f1;
      border-radius: 0.7rem;
      padding: 0.45rem;
      background: #fff;
      cursor: pointer;
      text-align: center;
    }
    .calendar-cell.dim { opacity: 0.55; }
    .calendar-cell.active { border-color: #0f4c5c; box-shadow: 0 0 0 1px #0f4c5c inset; }
    .calendar-cell .day { margin: 0; font-weight: 800; color: #0f4c5c; }
    .calendar-cell .meta { margin: 0.1rem 0 0; font-size: 0.7rem; color: #607287; text-align: center; }
    .calendar-detail-table th,
    .calendar-detail-table td { text-align: center; }
    .table-wrap { border: 1px solid #e1e8f1; border-radius: 0.8rem; overflow: auto; }
    table { width: 100%; min-width: 760px; border-collapse: collapse; font-size: 0.8rem; }
    th, td { padding: 0.45rem 0.5rem; border-bottom: 1px solid #edf1f6; text-align: left; }
    th { background: #0f4c5c; color: #fff; position: sticky; top: 0; }

    .cards {
      display: grid;
      gap: 0.7rem;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      min-height: 0;
      overflow: auto;
    }
    .card {
      border: 1px solid #dde5ef;
      border-radius: 0.95rem;
      padding: 0.7rem;
      text-decoration: none;
      color: inherit;
      display: grid;
      gap: 0.35rem;
      min-height: 165px;
      box-shadow: 0 8px 20px rgba(16, 37, 66, 0.08);
      transition: transform 180ms ease, box-shadow 180ms ease;
    }
    .card:hover {
      transform: translateY(-3px);
      box-shadow: 0 14px 24px rgba(16, 37, 66, 0.12);
    }
    .c0 { background: linear-gradient(165deg, #ffffff, #f0f8ff); }
    .c1 { background: linear-gradient(165deg, #ffffff, #f3fff5); }
    .c2 { background: linear-gradient(165deg, #ffffff, #fff8f0); }
    .c3 { background: linear-gradient(165deg, #ffffff, #f6f0ff); }
    .c4 { background: linear-gradient(165deg, #ffffff, #f0fffd); }
    .c5 { background: linear-gradient(165deg, #ffffff, #fff0f0); }
    .card-head { display: flex; align-items: center; gap: 0.55rem; }
    .icon-wrap {
      width: 2rem;
      height: 2rem;
      border-radius: 0.55rem;
      display: grid;
      place-items: center;
      background: #0f4c5c;
      color: #fff;
    }
    .index { margin: 0; font-size: 0.68rem; color: #607287; }
    h3 { margin: 0; font-size: 0.95rem; }
    .count { margin: 0.15rem 0; font-size: 1.35rem; font-weight: 800; color: #0f4c5c; }
    .error-count { color: #9b1c21; }
    .desc { margin: 0; color: #4e647b; font-size: 0.8rem; }
    .meta-row { margin-top: auto; display: flex; justify-content: space-between; align-items: center; gap: 0.4rem; }
    .pill {
      border: 1px solid #c9d9ea;
      border-radius: 999px;
      padding: 0.18rem 0.5rem;
      font-size: 0.7rem;
      font-weight: 700;
      background: #fff;
    }
    .error-pill { border-color: #e6b6b8; color: #9b1c21; }
    .status-error {
      margin: 0;
      font-size: 0.82rem;
      color: #9b1c21;
      border: 1px solid #e6b6b8;
      background: #fff5f5;
      border-radius: 0.7rem;
      padding: 0.45rem 0.6rem;
    }
    .open-label { font-size: 0.72rem; font-weight: 700; color: #0f4c5c; }
    @media (max-width: 760px) {
      .hero { flex-direction: column; }
      .pie-card { grid-template-columns: 1fr; }
      .pie-chart { width: 260px; height: 260px; margin: 0 auto; }
      .pie-legend { grid-template-columns: repeat(2, minmax(0, 1fr)); }
      .cards { grid-template-columns: 1fr; }
      .hero-actions { justify-items: start; width: 100%; }
    }
  `
})
export class DashboardPageComponent {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  readonly cards = signal<DashboardCard[]>([]);
  readonly stats = signal<AdminStats | null>(null);
  readonly statsError = signal('');
  readonly verification = signal<VerificationSummary | null>(null);
  readonly verificationError = signal('');
  readonly calendarDays = signal<CalendarDay[]>([]);
  readonly calendarError = signal('');
  readonly selectedDay = signal<CalendarDay | null>(null);
  readonly onlineUsers = signal<UserProfile[]>([]);
  readonly pendingUsers = signal<UserProfile[]>([]);
  readonly moduleSearch = signal('');
  readonly verificationSearch = signal('');
  readonly userSearch = signal('');
  selectedRange = 'weekly';
  calendarMonth = this.formatMonthOnly(new Date());
  verificationGranularity: 'moment' | 'day' | 'month' | 'year' = 'day';
  verificationAtMoment = this.formatDateTimeLocal(new Date());
  verificationAtDay = this.formatDateOnly(new Date());
  verificationAtMonth = this.formatMonthOnly(new Date());
  verificationAtYear = new Date().getUTCFullYear();

  private readonly colorClasses = ['c0', 'c1', 'c2', 'c3', 'c4', 'c5'];
  private readonly pieColors = ['#0f4c5c', '#2f8c88', '#f4a261', '#6174d3', '#d97a8c', '#6e9f4b', '#27a9c2', '#8b6ec7'];
  private readonly trendLabels = computed(() => {
    const stats = this.stats();
    if (!stats?.trend?.length) {
      return [];
    }

    if (stats.trendLabels?.length === stats.trend.length) {
      return stats.trendLabels;
    }

    return this.buildTrendLabels(stats.range, stats.trend.length);
  });

  constructor() {
    this.refreshAll();
  }

  refreshAll(): void {
    this.reload();
    this.loadStats();
    this.loadVerification();
    this.loadCalendarData();
    this.loadUserApprovalData();
  }

  reload(): void {
    const requests = RESOURCE_CONFIGS.map((resource) =>
      this.api.list(resource.listPath).pipe(
        map((rows) => ({ resource, count: rows.length, hasError: false })),
        catchError(() => of({ resource, count: 0, hasError: true }))
      )
    );

    forkJoin(requests).subscribe((results) => {
      const nextCards = results.map(({ resource, count, hasError }, index) => ({
        key: resource.key,
        icon: resource.icon,
        title: resource.title,
        summary: resource.summary ?? 'Management workspace.',
        count,
        countText: hasError ? 'N/A' : String(count),
        hasError,
        colorClass: this.colorClasses[index % this.colorClasses.length]
      }));

      this.cards.set(nextCards);
    });
  }

  cardTrend(): number[] {
    return this.cards().map((c) => c.count || 0);
  }

  loadStats(): void {
    this.statsError.set('');
    this.api
      .get(`/api/admin/stats?range=${this.selectedRange}`)
      .pipe(catchError((err) => {
        this.statsError.set(this.describeError('Stats', err));
        return of(null);
      }))
      .subscribe((data) => {
        this.stats.set((data as AdminStats | null) ?? null);
      });
  }

  loadVerification(): void {
    this.verificationError.set('');
    const atUtc = this.resolveVerificationAtUtc();
    const path = `/api/invoices/verification?granularity=${this.verificationGranularity}&atUtc=${encodeURIComponent(atUtc)}`;

    this.api
      .get(path)
      .pipe(catchError((err) => {
        this.verificationError.set(this.describeError('Verification', err));
        return of(null);
      }))
      .subscribe((data) => {
        this.verification.set((data as VerificationSummary | null) ?? null);
      });
  }

  loadCalendarData(): void {
    this.calendarError.set('');
    forkJoin([
      this.api.list('/api/reservations').pipe(catchError((err) => {
        this.calendarError.set(this.describeError('Reservations', err));
        return of([]);
      })),
      this.api.list('/api/invoices').pipe(catchError((err) => {
        this.calendarError.set(this.describeError('Invoices', err));
        return of([]);
      })),
      this.api.list('/api/payments').pipe(catchError((err) => {
        this.calendarError.set(this.describeError('Payments', err));
        return of([]);
      }))
    ]).subscribe(([reservations, invoices, payments]) => {
      const monthStart = new Date(`${this.calendarMonth}-01T00:00:00.000Z`);
      const gridStart = new Date(monthStart);
      gridStart.setUTCDate(1 - monthStart.getUTCDay());

      const days: CalendarDay[] = [];
      for (let i = 0; i < 42; i += 1) {
        const dayDate = new Date(gridStart);
        dayDate.setUTCDate(gridStart.getUTCDate() + i);
        const dayKey = this.formatDateOnly(dayDate);
        const dayReservations = (reservations as any[]).filter((r) => this.formatDateOnly(new Date(r.createdAt ?? r.createdDate ?? Date.now())) === dayKey);
        const dayInvoices = (invoices as any[]).filter((inv) => this.formatDateOnly(new Date(inv.issuedAt ?? inv.issuedDate ?? Date.now())) === dayKey);
        const dayInvoiceIds = new Set(dayInvoices.map((inv) => Number(inv.id ?? 0)));
        const dayPayments = (payments as any[]).filter((pay) => {
          const processed = pay.processedAt ? this.formatDateOnly(new Date(pay.processedAt)) === dayKey : false;
          return processed || dayInvoiceIds.has(Number(pay.invoiceId ?? -1));
        });
        const sales = dayPayments
          .filter((p) => String(p.status ?? '').toLowerCase() === 'paid')
          .reduce((sum, p) => sum + Number(p.amount ?? 0), 0);

        days.push({
          date: dayKey,
          dayNumber: dayDate.getUTCDate(),
          inCurrentMonth: dayDate.getUTCMonth() === monthStart.getUTCMonth() && dayDate.getUTCFullYear() === monthStart.getUTCFullYear(),
          reservations: dayReservations,
          invoices: dayInvoices,
          payments: dayPayments,
          sales
        });
      }

      this.calendarDays.set(days);
      const firstCurrent = days.find((d) => d.inCurrentMonth) ?? days[0] ?? null;
      this.selectedDay.set(firstCurrent);
    });
  }

  selectCalendarDay(day: CalendarDay): void {
    this.selectedDay.set(day);
  }

  dayReservationRefs(day: CalendarDay): string {
    const refs = day.reservations
      .slice(0, 3)
      .map((r) => String(r?.reference ?? r?.reservationId ?? '').trim())
      .filter((v) => !!v);
    return refs.join(', ') || '-';
  }

  loadUserApprovalData(): void {
    this.onlineUsers.set(this.auth.listOnlineUsers());
    const localPending = this.auth.listPendingUsers();
    this.api.list('/api/accounts').pipe(catchError(() => of([]))).subscribe((rows) => {
      const apiPending = (rows as any[])
        .filter((row) => row?.isActive === false)
        .map((row) => ({
          id: String(row?.id ?? ''),
          role: (String(row?.role ?? 'regular').toLowerCase() === 'super' ? 'super' : 'regular') as UserProfile['role'],
          firstName: String(row?.firstName ?? ''),
          lastName: String(row?.lastName ?? ''),
          email: String(row?.email ?? ''),
          phone: String(row?.phone ?? ''),
          preferredCurrency: 'USD' as const,
          avatarUrl: ''
        }));

      const merged = [...localPending, ...apiPending].reduce<UserProfile[]>((acc, user) => {
        if (!acc.some((u) => u.email.toLowerCase() === user.email.toLowerCase())) {
          acc.push(user);
        }
        return acc;
      }, []);

      this.pendingUsers.set(merged);
    });
  }

  trendPieSegments(points: number[]): PieSegment[] {
    const labels = this.resolveTrendLabels(points.length);
    return this.buildPieSegments(points, labels);
  }

  bookingPieSegments(points: number[]): PieSegment[] {
    const labels = this.resolveTrendLabels(points.length);
    const bookingValues = points.map((value) => Number((value * 0.7).toFixed(2)));
    return this.buildPieSegments(bookingValues, labels);
  }

  cardPieSegments(): PieSegment[] {
    const cards = this.cards();
    const labels = cards.map((card) => card.title);
    const values = cards.map((card) => card.count);
    return this.buildPieSegments(values, labels);
  }

  pieTooltip(segment: PieSegment): string {
    return `${segment.label}: ${segment.value} (${segment.percent.toFixed(1)}%)`;
  }

  filteredCards(): DashboardCard[] {
    const query = this.moduleSearch().trim().toLowerCase();
    if (!query) {
      return this.cards();
    }

    return this.cards().filter((card) =>
      `${card.title} ${card.summary} ${card.key}`.toLowerCase().includes(query)
    );
  }

  filteredVerificationItems(items: VerificationItem[]): VerificationItem[] {
    const query = this.verificationSearch().trim().toLowerCase();
    if (!query) {
      return items;
    }

    return items.filter((item) =>
      `${item.invoiceId} ${item.reservationId} ${item.invoiceStatus} ${item.lastPaymentStatus} ${item.currency}`
        .toLowerCase()
        .includes(query)
    );
  }

  filteredOnlineUsers(): UserProfile[] {
    return this.filterUsers(this.onlineUsers());
  }

  filteredPendingUsers(): UserProfile[] {
    return this.filterUsers(this.pendingUsers());
  }

  private buildPieSegments(values: number[], labels: string[]): PieSegment[] {
    const normalized = values
      .map((value, index) => ({ label: labels[index] ?? `Slice ${index + 1}`, value: Number(value) || 0 }))
      .filter((item) => item.value > 0);

    const total = normalized.reduce((sum, item) => sum + item.value, 0);
    if (!total) {
      return [];
    }

    let offset = 0;
    return normalized.map((item, index) => {
      const percent = (item.value / total) * 100;
      const segment: PieSegment = {
        label: item.label,
        value: item.value,
        percent,
        color: this.pieColors[index % this.pieColors.length],
        dasharray: `${percent} ${100 - percent}`,
        dashoffset: -offset
      };
      offset += percent;
      return segment;
    });
  }

  private resolveTrendLabels(count: number): string[] {
    const labels = this.trendLabels();
    if (labels.length === count) {
      return labels;
    }

    return Array.from({ length: count }, (_, index) => `Slice ${index + 1}`);
  }

  private buildTrendLabels(range: string, count: number): string[] {
    if (count <= 0) {
      return [];
    }

    const now = new Date();
    const normalizedRange = (range || this.selectedRange || 'weekly').toLowerCase();
    const start = this.resolveTrendStart(normalizedRange, now);
    const stepMs = Math.max(Math.floor((now.getTime() - start.getTime()) / count), 1);

    return Array.from({ length: count }, (_, index) => {
      const bucketStart = new Date(start.getTime() + stepMs * index);
      const bucketEnd = new Date(
        index === count - 1 ? now.getTime() : start.getTime() + stepMs * (index + 1)
      );
      return this.formatTrendLabel(normalizedRange, bucketStart, bucketEnd);
    });
  }

  private resolveTrendStart(range: string, now: Date): Date {
    const start = new Date(now);
    switch (range) {
      case 'daily':
        start.setHours(0, 0, 0, 0);
        return start;
      case 'monthly':
        start.setDate(start.getDate() - 30);
        return start;
      case 'quarterly':
        start.setMonth(start.getMonth() - 3);
        return start;
      case 'yearly':
        start.setFullYear(start.getFullYear() - 1);
        return start;
      case 'weekly':
      default:
        start.setDate(start.getDate() - 7);
        return start;
    }
  }

  private formatTrendLabel(range: string, start: Date, end: Date): string {
    if (range === 'daily') {
      const startHour = String(start.getHours()).padStart(2, '0');
      const endHour = String(end.getHours()).padStart(2, '0');
      return `${startHour}:00-${endHour}:00`;
    }

    if (range === 'weekly') {
      return start.toLocaleDateString(undefined, { weekday: 'short' });
    }

    if (range === 'yearly') {
      return start.toLocaleDateString(undefined, { month: 'short' });
    }

    const startLabel = start.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
    const endLabel = end.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
    return startLabel === endLabel ? startLabel : `${startLabel}-${endLabel}`;
  }

  private filterUsers(users: UserProfile[]): UserProfile[] {
    const query = this.userSearch().trim().toLowerCase();
    if (!query) {
      return users;
    }

    return users.filter((user) =>
      `${user.firstName} ${user.lastName} ${user.email}`.toLowerCase().includes(query)
    );
  }

  private resolveVerificationAtUtc(): string {
    if (this.verificationGranularity === 'moment') {
      return new Date(this.verificationAtMoment || new Date().toISOString()).toISOString();
    }

    if (this.verificationGranularity === 'day') {
      return new Date(`${this.verificationAtDay}T00:00:00.000Z`).toISOString();
    }

    if (this.verificationGranularity === 'month') {
      return new Date(`${this.verificationAtMonth}-01T00:00:00.000Z`).toISOString();
    }

    return new Date(`${this.verificationAtYear}-01-01T00:00:00.000Z`).toISOString();
  }

  private describeError(section: string, err: any): string {
    const fromApi = err?.error?.error?.detail ?? err?.error?.error?.title ?? err?.error?.message ?? err?.message;
    return fromApi ? `${section} unavailable: ${fromApi}` : `${section} unavailable.`;
  }

  private formatDateOnly(value: Date): string {
    const year = value.getUTCFullYear();
    const month = String(value.getUTCMonth() + 1).padStart(2, '0');
    const day = String(value.getUTCDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private formatMonthOnly(value: Date): string {
    const year = value.getUTCFullYear();
    const month = String(value.getUTCMonth() + 1).padStart(2, '0');
    return `${year}-${month}`;
  }

  private formatDateTimeLocal(value: Date): string {
    const year = value.getUTCFullYear();
    const month = String(value.getUTCMonth() + 1).padStart(2, '0');
    const day = String(value.getUTCDate()).padStart(2, '0');
    const hours = String(value.getUTCHours()).padStart(2, '0');
    const minutes = String(value.getUTCMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

}
