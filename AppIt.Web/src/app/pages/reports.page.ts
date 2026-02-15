import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { jsPDF } from 'jspdf';
import { ApiService } from '../api.service';
import { AuthService } from '../auth.service';

interface DatasetPreset {
  label: string;
  path: string;
  reportKey: string;
  reportTitle: string;
}

interface SnapshotListItem {
  id: number;
  reportKey: string;
  title: string;
  snapshotDate: string;
}

interface SnapshotDetail extends SnapshotListItem {
  dataJson: string;
  generatedByUserId: number;
}

interface ReportExportMeta {
  dataset: string;
  reportKey: string;
  reportTitle: string;
  generatedAtIso: string;
  generatedDate: string;
  generatedTime: string;
  generatedDay: string;
  generatedBy: string;
}

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="reports">
      <header class="report-header">
        <div>
          <p class="eyebrow">Business Intelligence</p>
          <h1><span class="material-symbols-outlined">insights</span> Report Studio</h1>
          <p>Generate datasets, save snapshots, and export in JSON or PDF.</p>
        </div>
        <div class="header-actions">
          <button class="btn-base btn-primary" (click)="generate()">Generate</button>
          <button class="btn-base btn-secondary" (click)="saveSnapshot()" [disabled]="!reportRows().length">Save Snapshot</button>
          <button class="btn-base btn-secondary" (click)="exportJson()" [disabled]="!reportRows().length">JSON</button>
          <button class="btn-base btn-secondary" (click)="exportPdf()" [disabled]="!reportRows().length">PDF</button>
        </div>
      </header>

      <article class="panel controls">
        <h2>Report Controls</h2>
        <div class="control-grid">
          <div class="control-item">
            <label>Dataset</label>
            <select class="app-select" [(ngModel)]="selectedDataset" (ngModelChange)="onDatasetChanged($event)">
              <option *ngFor="let ds of datasets" [value]="ds.path">{{ ds.label }}</option>
            </select>
          </div>
          <div class="control-item">
            <label>Report Type (Key)</label>
            <p class="auto-value">{{ reportKey }}</p>
          </div>
          <div class="control-item">
            <label>Title</label>
            <p class="auto-value">{{ reportTitle }}</p>
          </div>
          <div class="control-item">
            <label>Filter Report Rows</label>
            <input
              class="app-input"
              [ngModel]="rowFilter()"
              (ngModelChange)="rowFilter.set($event)"
              placeholder="Search report values..."
            />
          </div>
          <div class="control-item" *ngIf="isSuperUser()">
            <label>Filter Snapshots</label>
            <input
              class="app-input"
              [ngModel]="snapshotFilter()"
              (ngModelChange)="snapshotFilter.set($event)"
              placeholder="Search snapshot id, key, title..."
            />
          </div>
        </div>
      </article>

      <div class="grid top-grid">
        <article class="panel metrics-panel" *ngIf="metrics() as m">
          <h2>Report Metrics</h2>
          <div class="metrics">
            <div class="metric-card m1">
              <p class="label">Rows</p>
              <p class="value">{{ m.rowCount }}</p>
            </div>
            <div class="metric-card m2">
              <p class="label">Numeric Total</p>
              <p class="value">{{ m.numericTotal | number:'1.0-2' }}</p>
            </div>
            <div class="metric-card m3">
              <p class="label">Generated At</p>
              <p class="value">{{ m.generatedAt | date:'medium' }}</p>
            </div>
          </div>
        </article>

        <article class="panel snapshots-panel" *ngIf="isSuperUser()">
          <div class="snapshot-head">
            <h2>Snapshots</h2>
            <button class="btn-base btn-secondary btn-compact" (click)="loadSnapshots()">Refresh</button>
          </div>

          <div class="table-wrap">
            <table *ngIf="filteredSnapshots().length; else noSnapshots">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Report Key</th>
                  <th>Title</th>
                  <th>Snapshot Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let snapshot of filteredSnapshots()">
                  <td>{{ snapshot.id }}</td>
                  <td>{{ snapshot.reportKey }}</td>
                  <td>{{ snapshot.title }}</td>
                  <td>{{ snapshot.snapshotDate | date:'short' }}</td>
                  <td class="actions">
                    <button class="btn-base btn-secondary btn-compact" (click)="readSnapshot(snapshot.id)">Read</button>
                    <button class="btn-base btn-danger btn-compact" (click)="deleteSnapshot(snapshot.id)">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
            <ng-template #noSnapshots>
              <p class="empty">No snapshots available.</p>
            </ng-template>
          </div>
        </article>
      </div>

      <article class="panel data-panel">
        <h2>Report Preview</h2>
        <div class="table-wrap">
          <table *ngIf="filteredReportRows().length; else noReportRows">
            <thead>
              <tr>
                <th>#</th>
                <th *ngFor="let col of reportColumns()">{{ col }}</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let row of filteredReportRows(); let i = index">
                <td class="row-index">{{ i + 1 }}</td>
                <td *ngFor="let col of reportColumns()">{{ toDisplay(row[col]) }}</td>
              </tr>
            </tbody>
          </table>
          <ng-template #noReportRows>
            <p class="empty">No report data loaded yet.</p>
          </ng-template>
        </div>
      </article>

      <p class="status" *ngIf="status()">{{ status() }}</p>
    </section>
  `,
  styles: `
    .reports {
      display: grid;
      gap: 0.7rem;
      height: 100%;
      min-height: 0;
      grid-template-rows: auto auto auto 1fr auto;
      overflow: auto;
      align-content: start;
    }
    .report-header {
      border: 1px solid #d9e4f1;
      border-radius: 1rem;
      padding: 0.85rem;
      background: linear-gradient(150deg, #ffffff, #f4f9ff);
      display: flex;
      justify-content: space-between;
      gap: 0.8rem;
      align-items: flex-start;
    }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.09em; font-size: 0.72rem; color: #0f4c5c; font-weight: 700; }
    .report-header h1 { margin: 0.2rem 0; display: flex; align-items: center; gap: 0.35rem; font-size: 1.2rem; }
    .report-header p { margin: 0; color: #5f6f82; font-size: 0.9rem; }
    .header-actions { display: flex; gap: 0.4rem; flex-wrap: wrap; justify-content: flex-end; }

    .grid { display: grid; gap: 0.8rem; min-height: 0; }
    .top-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }

    .panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      padding: 0.75rem;
      background: #fff;
      min-height: 0;
      overflow: hidden;
      display: grid;
      grid-template-rows: auto 1fr;
      gap: 0.55rem;
    }
    .panel h2 { margin: 0; font-size: 1rem; color: #14334f; }

    .controls { background: linear-gradient(165deg, #ffffff, #f5fbff); }
    .metrics-panel { background: linear-gradient(165deg, #ffffff, #f8fff7); }

    .control-grid {
      display: grid;
      grid-template-columns: repeat(3, minmax(220px, 1fr));
      gap: 0.55rem;
      align-items: stretch;
    }
    .control-item {
      border: 1px solid #e0eaf6;
      border-radius: 0.7rem;
      padding: 0.5rem;
      background: #ffffff;
      min-height: 74px;
    }
    label { display: block; font-weight: 700; margin: 0 0 0.25rem; font-size: 0.82rem; color: #43576d; }
    .auto-value {
      margin: 0;
      min-height: 2.15rem;
      display: grid;
      align-items: center;
      padding: 0.35rem 0.45rem;
      border: 1px solid #d8e4f4;
      border-radius: 0.55rem;
      background: #f7fbff;
      color: #24415d;
      font-weight: 700;
      font-size: 0.82rem;
    }

    .metrics { display: grid; grid-template-columns: 1fr; gap: 0.5rem; }
    .metric-card {
      border: 1px solid #e4edf8;
      border-radius: 0.75rem;
      padding: 0.55rem;
      background: #fff;
    }
    .m1 { border-left: 5px solid #0f4c5c; }
    .m2 { border-left: 5px solid #2f8c88; }
    .m3 { border-left: 5px solid #f4a261; }
    .label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.09em; color: #607287; margin: 0; }
    .value { font-size: 0.95rem; color: #102542; margin: 0.2rem 0 0; font-weight: 700; }

    .snapshot-head {
      display: flex;
      justify-content: space-between;
      gap: 0.6rem;
      align-items: center;
    }

    .data-panel {
      background: linear-gradient(165deg, #ffffff, #f3f8ff);
      min-height: 0;
    }
    .table-wrap {
      min-height: 0;
      overflow: auto;
      border: 1px solid #dfeaf6;
      border-radius: 0.7rem;
      background: #fff;
    }
    table { width: 100%; min-width: 100%; border-collapse: collapse; font-size: 0.8rem; table-layout: fixed; }
    th, td {
      text-align: left;
      padding: 0.45rem 0.5rem;
      border-bottom: 1px solid #edf1f6;
      vertical-align: top;
      white-space: normal;
      word-break: break-word;
    }
    th {
      position: sticky;
      top: 0;
      background: #0f4c5c;
      color: #ffffff;
      font-weight: 700;
    }
    tbody tr:nth-child(even) { background: #f7fbff; }
    tbody tr:hover { background: #eef6ff; }
    .row-index {
      width: 42px;
      font-weight: 700;
      color: #0f4c5c;
      text-align: center;
      background: #eef6ff;
    }
    .actions {
      white-space: nowrap;
      display: flex;
      gap: 0.35rem;
      flex-wrap: wrap;
    }
    .empty { margin: 0; color: #6d8096; padding: 0.8rem; }
    .status { margin: 0; background: #eff9f8; color: #0f4c5c; border: 1px solid #b8e1d9; border-radius: 0.7rem; padding: 0.45rem 0.7rem; font-size: 0.86rem; }

    @media (max-width: 1200px) {
      .reports { height: auto; grid-template-rows: auto; }
      .top-grid { grid-template-columns: 1fr; }
      .control-grid { grid-template-columns: 1fr; }
    }
    @media (max-width: 760px) {
      .report-header { flex-direction: column; }
      .header-actions { justify-content: flex-start; }
      .metrics { grid-template-columns: 1fr; }
    }
  `
})
export class ReportsPageComponent {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  readonly datasets: DatasetPreset[] = [
    {
      label: 'Accounts',
      path: '/api/accounts',
      reportKey: 'accounts-summary',
      reportTitle: 'User Accounts and Access Matrix'
    },
    {
      label: 'Products',
      path: '/api/products',
      reportKey: 'sales-products',
      reportTitle: 'Product Performance and Pricing'
    },
    {
      label: 'Accommodations',
      path: '/api/accommodations',
      reportKey: 'accommodation-utilization',
      reportTitle: 'Accommodation & Room Mix'
    },
    {
      label: 'Activities',
      path: '/api/activities',
      reportKey: 'operations-overview',
      reportTitle: 'Operations Daily Summary'
    },
    {
      label: 'Reservations',
      path: '/api/reservations',
      reportKey: 'reservation-utilization',
      reportTitle: 'Reservation Pipeline Report'
    },
    {
      label: 'Invoices',
      path: '/api/invoices',
      reportKey: 'invoice-financials',
      reportTitle: 'Invoice and Revenue Snapshot'
    },
    {
      label: 'Payments',
      path: '/api/payments',
      reportKey: 'payment-status',
      reportTitle: 'Payment Status Overview'
    },
    {
      label: 'Vouchers',
      path: '/api/vouchers',
      reportKey: 'operations-overview',
      reportTitle: 'Operations Daily Summary'
    }
  ];

  selectedDataset = this.datasets[0].path;
  reportKey = this.datasets[0].reportKey;
  reportTitle = this.datasets[0].reportTitle;

  readonly reportRows = signal<any[]>([]);
  readonly status = signal('');
  readonly metrics = signal<{ rowCount: number; numericTotal: number; generatedAt: string } | null>(null);
  readonly snapshots = signal<SnapshotListItem[]>([]);
  readonly rowFilter = signal('');
  readonly snapshotFilter = signal('');
  readonly isSuperUser = signal(this.auth.isSuperUser());

  constructor() {
    this.applyDatasetDefaults(this.selectedDataset);
    if (this.isSuperUser()) {
      this.loadSnapshots();
    }
  }

  onDatasetChanged(path: string): void {
    this.applyDatasetDefaults(path);
  }

  generate(): void {
    this.api.list(this.selectedDataset).subscribe({
      next: (rows) => {
        this.reportRows.set(rows);
        this.metrics.set({
          rowCount: rows.length,
          numericTotal: this.calculateNumericTotal(rows),
          generatedAt: new Date().toISOString()
        });
        this.status.set(`Loaded ${rows.length} rows from ${this.selectedDataset}.`);
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  loadSnapshots(): void {
    if (!this.isSuperUser()) {
      return;
    }

    this.api.list('/api/report-snapshots').subscribe({
      next: (rows) => this.snapshots.set((rows ?? []) as SnapshotListItem[]),
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  readSnapshot(id: number): void {
    if (!this.isSuperUser()) {
      return;
    }

    this.api.get(`/api/report-snapshots/${id}`).subscribe({
      next: (row) => {
        const detail = row as SnapshotDetail;
        const parsed = this.parseSnapshotRows(detail.dataJson);
        this.reportRows.set(parsed);
        this.metrics.set({
          rowCount: parsed.length,
          numericTotal: this.calculateNumericTotal(parsed),
          generatedAt: detail.snapshotDate
        });

        this.reportKey = detail.reportKey;
        this.reportTitle = detail.title;
        const dataset = this.datasets.find((item) => item.reportKey === detail.reportKey);
        if (dataset) {
          this.selectedDataset = dataset.path;
        }

        this.status.set(`Loaded snapshot #${detail.id}.`);
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  deleteSnapshot(id: number): void {
    if (!this.isSuperUser()) {
      return;
    }

    if (!window.confirm(`Delete snapshot #${id}?`)) {
      return;
    }

    this.api.delete(`/api/report-snapshots/${id}`).subscribe({
      next: () => {
        this.snapshots.update((rows) => rows.filter((row) => row.id !== id));
        this.status.set(`Snapshot #${id} deleted.`);
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  saveSnapshot(): void {
    const payload = {
      reportKey: this.reportKey,
      title: this.reportTitle,
      dataJson: JSON.stringify(this.reportRows(), null, 2),
      snapshotDate: new Date().toISOString(),
      generatedByUserId: this.resolveGeneratedByUserId()
    };

    this.api.post('/api/report-snapshots', payload).subscribe({
      next: () => {
        this.status.set('Snapshot saved successfully.');
        this.loadSnapshots();
      },
      error: (err) => this.status.set(this.describeError(err))
    });
  }

  exportJson(): void {
    const generatedAt = new Date();
    const meta = this.buildReportMeta(generatedAt);
    const payload = {
      meta,
      columns: this.reportColumns(),
      rows: this.reportRows()
    };
    const fileName = `${this.escapeFileSegment(this.reportTitle)}-${this.formatFileTimestamp(generatedAt)}.json`;
    this.downloadFile(fileName, 'application/json', JSON.stringify(payload, null, 2));
  }

  exportPdf(): void {
    const rows = this.reportRows();
    if (!rows.length) {
      return;
    }

    const generatedAt = new Date();
    const meta = this.buildReportMeta(generatedAt);
    const columns = this.reportColumns();
    const orientation = columns.length > 6 ? 'landscape' : 'portrait';
    const doc = new jsPDF({ orientation });
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();
    const margin = 10;
    const tableWidth = pageWidth - margin * 2;
    const indexWidth = 11;
    const colWidth = (tableWidth - indexWidth) / Math.max(columns.length, 1);
    const rowHeight = 7;
    const bodyFontSize = columns.length > 8 ? 7 : 8;

    doc.setFillColor(18, 46, 83);
    doc.rect(0, 0, pageWidth, 42, 'F');
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(12);
    doc.text(meta.reportTitle || 'Report', margin, 9);
    doc.setFontSize(8);
    doc.text(`Dataset: ${meta.dataset}`, margin, 15);
    doc.text(`Report Key: ${meta.reportKey}`, margin, 20);
    doc.text(`Report Time: ${meta.generatedTime}`, margin, 25);
    doc.text(`Generated: ${meta.generatedDate} ${meta.generatedTime} (${meta.generatedDay})`, margin, 30);
    doc.text(`Generated By: ${meta.generatedBy}`, margin, 35);
    doc.text(`Generated At (ISO): ${meta.generatedAtIso}`, margin, 40);

    let y = 48;
    const drawHeader = () => {
      doc.setFillColor(15, 118, 138);
      doc.setTextColor(255, 255, 255);
      doc.setFontSize(8);

      doc.rect(margin, y, indexWidth, rowHeight, 'F');
      doc.text('#', margin + 2, y + 4.8);

      columns.forEach((col, index) => {
        const x = margin + indexWidth + index * colWidth;
        doc.rect(x, y, colWidth, rowHeight, 'F');
        const title = col.length > 20 ? `${col.slice(0, 20)}...` : col;
        doc.text(title, x + 1.1, y + 4.8, { maxWidth: colWidth - 2 });
      });
      y += rowHeight;
    };

    drawHeader();
    doc.setTextColor(20, 36, 54);
    doc.setFontSize(bodyFontSize);
    rows.forEach((row, rowIndex) => {
      if (y + rowHeight > pageHeight - margin) {
        doc.addPage();
        y = 12;
        drawHeader();
        doc.setTextColor(20, 36, 54);
        doc.setFontSize(bodyFontSize);
      }

      const rowFill = rowIndex % 2 === 0 ? [239, 248, 251] : [255, 255, 255];
      doc.setFillColor(rowFill[0], rowFill[1], rowFill[2]);
      doc.rect(margin, y, indexWidth, rowHeight, 'F');
      doc.setDrawColor(210, 224, 236);
      doc.rect(margin, y, indexWidth, rowHeight, 'S');
      doc.text(String(rowIndex + 1), margin + 1.5, y + 4.8, { maxWidth: indexWidth - 2 });

      columns.forEach((col, colIndex) => {
        const x = margin + indexWidth + colIndex * colWidth;
        doc.setFillColor(rowFill[0], rowFill[1], rowFill[2]);
        doc.rect(x, y, colWidth, rowHeight, 'F');
        doc.setDrawColor(210, 224, 236);
        doc.rect(x, y, colWidth, rowHeight, 'S');
        const raw = this.toDisplay(row[col]);
        const value = raw.length > 30 ? `${raw.slice(0, 30)}...` : raw;
        doc.text(value, x + 1.1, y + 4.8, { maxWidth: colWidth - 2 });
      });

      y += rowHeight;
    });

    const fileName = `${this.escapeFileSegment(this.reportTitle)}-${this.formatFileTimestamp(generatedAt)}.pdf`;
    doc.save(fileName);
  }

  reportColumns(): string[] {
    const first = this.reportRows()[0];
    return first ? Object.keys(first).slice(0, 10) : [];
  }

  filteredReportRows(): any[] {
    const rows = this.reportRows();
    const query = this.rowFilter().trim().toLowerCase();
    if (!query) {
      return rows;
    }

    return rows.filter((row) =>
      Object.entries(row ?? {}).some(([key, value]) => {
        if (value === null || value === undefined || typeof value === 'object') {
          return false;
        }
        return `${key} ${String(value)}`.toLowerCase().includes(query);
      })
    );
  }

  filteredSnapshots(): SnapshotListItem[] {
    const rows = this.snapshots();
    const query = this.snapshotFilter().trim().toLowerCase();
    if (!query) {
      return rows;
    }

    return rows.filter((row) =>
      `${row.id} ${row.reportKey} ${row.title} ${row.snapshotDate}`.toLowerCase().includes(query)
    );
  }

  toDisplay(value: unknown): string {
    if (value === null || value === undefined || value === '') {
      return '-';
    }
    if (typeof value === 'object') {
      return JSON.stringify(value);
    }
    return String(value);
  }

  private applyDatasetDefaults(path: string): void {
    const preset = this.datasets.find((item) => item.path === path);
    if (!preset) {
      return;
    }

    this.reportKey = preset.reportKey;
    this.reportTitle = preset.reportTitle;
  }

  private resolveGeneratedByUserId(): number {
    const rawId = String(this.auth.user()?.id ?? '').trim();
    const digits = rawId.replace(/\D/g, '');
    const parsed = digits ? Number(digits.slice(-9)) : 1;
    if (!Number.isFinite(parsed) || parsed <= 0) {
      return 1;
    }
    return Math.min(parsed, 2147483647);
  }

  private parseSnapshotRows(dataJson: string): any[] {
    if (!dataJson?.trim()) {
      return [];
    }

    try {
      const parsed = JSON.parse(dataJson);
      if (parsed && typeof parsed === 'object' && Array.isArray((parsed as any).rows)) {
        return (parsed as any).rows as any[];
      }
      return Array.isArray(parsed) ? parsed : [parsed];
    } catch {
      return [];
    }
  }

  private buildReportMeta(generatedAt: Date): ReportExportMeta {
    const dataset = this.datasets.find((item) => item.path === this.selectedDataset)?.label ?? this.selectedDataset;
    const user = this.auth.user();
    const generatedBy = user ? `${user.firstName} ${user.lastName}`.trim() || user.email : 'Unknown User';

    return {
      dataset,
      reportKey: this.reportKey,
      reportTitle: this.reportTitle,
      generatedAtIso: generatedAt.toISOString(),
      generatedDate: generatedAt.toLocaleDateString(),
      generatedTime: generatedAt.toLocaleTimeString(),
      generatedDay: generatedAt.toLocaleDateString(undefined, { weekday: 'long' }),
      generatedBy
    };
  }

  private formatFileTimestamp(value: Date): string {
    const yyyy = value.getFullYear();
    const mm = String(value.getMonth() + 1).padStart(2, '0');
    const dd = String(value.getDate()).padStart(2, '0');
    const hh = String(value.getHours()).padStart(2, '0');
    const min = String(value.getMinutes()).padStart(2, '0');
    const ss = String(value.getSeconds()).padStart(2, '0');
    return `${yyyy}${mm}${dd}-${hh}${min}${ss}`;
  }

  private escapeFileSegment(input: string): string {
    return (input || 'report')
      .trim()
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .slice(0, 60) || 'report';
  }

  private calculateNumericTotal(rows: any[]): number {
    let total = 0;
    rows.forEach((row) => {
      Object.values(row).forEach((value) => {
        if (typeof value === 'number' && Number.isFinite(value)) {
          total += value;
        }
      });
    });

    return total;
  }

  private downloadFile(fileName: string, mimeType: string, content: string): void {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  }

  private describeError(err: any): string {
    const fromApi = err?.error?.message ?? err?.error?.title ?? err?.message;
    return fromApi ? `Request failed: ${fromApi}` : 'Request failed';
  }
}
