import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';
import { ApiService } from '../api.service';
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
          <button class="btn-base btn-secondary" (click)="reload()">
            <span class="material-symbols-outlined">refresh</span>
            Refresh All
          </button>
        </div>
      </header>

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
            <p class="eyebrow">Trend</p>
            <h2>Revenue vs Bookings</h2>
          </div>
          <span class="pill">Range: {{ s.range }}</span>
        </div>
        <svg viewBox="0 0 300 120" preserveAspectRatio="none">
          <polyline [attr.points]="buildLine(s.trend)" fill="none" stroke="#0f4c5c" stroke-width="3"></polyline>
          <polyline [attr.points]="buildComparisonLine(s.trend)" fill="none" stroke="#f4a261" stroke-width="3"></polyline>
        </svg>
      </section>

      <section class="cards">
        <a
          class="card"
          [ngClass]="card.colorClass"
          *ngFor="let card of cards(); let i = index"
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
    .dashboard { display: grid; gap: 1rem; height: 100%; min-height: 0; grid-template-rows: auto auto auto 1fr; }
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
    svg { width: 100%; height: 160px; background: #fff; border-radius: 0.7rem; border: 1px solid #e2eaf4; }

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
    .open-label { font-size: 0.72rem; font-weight: 700; color: #0f4c5c; }
    @media (max-width: 760px) {
      .hero { flex-direction: column; }
      .cards { grid-template-columns: 1fr; }
      .hero-actions { justify-items: start; width: 100%; }
    }
  `
})
export class DashboardPageComponent {
  private readonly api = inject(ApiService);
  readonly cards = signal<DashboardCard[]>([]);
  readonly stats = signal<AdminStats | null>(null);
  selectedRange = 'weekly';

  private readonly colorClasses = ['c0', 'c1', 'c2', 'c3', 'c4', 'c5'];

  constructor() {
    this.reload();
    this.loadStats();
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

  loadStats(): void {
    this.api
      .get(`/api/admin/stats?range=${this.selectedRange}`)
      .pipe(catchError(() => of(this.sampleStats(this.selectedRange))))
      .subscribe((data) => this.stats.set(data as AdminStats));
  }

  buildLine(points: number[]): string {
    if (!points.length) {
      return '';
    }
    const max = Math.max(...points, 1);
    return points
      .map((value, index) => {
        const x = (index / (points.length - 1)) * 300;
        const y = 110 - (value / max) * 90;
        return `${x.toFixed(1)},${y.toFixed(1)}`;
      })
      .join(' ');
  }

  buildComparisonLine(points: number[]): string {
    return this.buildLine(points.map((value) => value * 0.7));
  }

  private sampleStats(range: string): AdminStats {
    const trend = [20, 32, 28, 40, 36, 52, 48];
    return {
      range,
      totalSales: 482,
      totalBookings: 210,
      totalEarnings: 126000,
      totalCustomers: 340,
      trend
    };
  }
}
