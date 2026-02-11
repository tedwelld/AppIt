import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../api.service';
import { buildPath, RESOURCE_MAP } from '../entity-config';
import { FieldDef, ResourceConfig } from '../models';

@Component({
  selector: 'app-entities-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  template: `
    <section class="entity-page" *ngIf="config() as cfg; else missingResource">
      <header class="toolbar">
        <div>
          <p class="eyebrow">Data Management</p>
          <h1><span class="material-symbols-outlined">{{ cfg.icon }}</span> {{ cfg.title }}</h1>
          <p>{{ cfg.summary }}</p>
        </div>
        <div class="toolbar-actions">
          <input
            class="app-input"
            type="search"
            [ngModel]="query()"
            (ngModelChange)="query.set($event)"
            placeholder="Search records"
          />
          <button class="btn-base btn-secondary" (click)="clearSearch()">Clear Search</button>
          <button class="btn-base btn-secondary" (click)="reload()">Refresh</button>
          <button class="btn-base btn-primary" (click)="startCreate()" [disabled]="cfg.readOnly">New</button>
        </div>
      </header>

      <p class="status" *ngIf="message()">{{ message() }}</p>

      <div class="content-grid">
        <section class="table-panel">
          <div class="table-header-actions">
            <button class="btn-base btn-secondary btn-compact" (click)="duplicateSelected()" [disabled]="!selected() || cfg.readOnly">
              Duplicate Selected
            </button>
            <button class="btn-base btn-danger btn-compact" (click)="deleteSelected()" [disabled]="!selected() || cfg.readOnly || !cfg.deletePath">
              Delete Selected
            </button>
          </div>

          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th *ngFor="let col of columns()">{{ col }}</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  *ngFor="let row of filteredRows()"
                  [class.selected-row]="isSelected(row)"
                  (click)="selectRow(row)"
                >
                  <td *ngFor="let col of columns()">{{ toDisplay(row[col]) }}</td>
                  <td class="actions">
                    <button class="btn-base btn-secondary btn-compact" (click)="edit(row); $event.stopPropagation()" [disabled]="cfg.readOnly">Edit</button>
                    <button class="btn-base btn-danger btn-compact" (click)="remove(row); $event.stopPropagation()" [disabled]="cfg.readOnly || !cfg.deletePath">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
            <p class="empty" *ngIf="!filteredRows().length">No records found.</p>
          </div>
        </section>

        <aside class="panel" *ngIf="!cfg.readOnly">
          <h2>{{ editing() ? 'Edit' : 'Create' }} {{ cfg.title }}</h2>
          <form [formGroup]="form" (ngSubmit)="save()">
            <div class="form-scroll">
              <div class="form-grid">
                <div class="field" *ngFor="let field of cfg.fields">
                  <label [for]="field.key">{{ field.label }}</label>

                  <input
                    class="app-input"
                    *ngIf="field.type !== 'textarea' && field.type !== 'checkbox'"
                    [id]="field.key"
                    [type]="field.type"
                    [formControlName]="field.key"
                  />

                  <textarea
                    class="app-textarea"
                    *ngIf="field.type === 'textarea'"
                    [id]="field.key"
                    [formControlName]="field.key"
                    rows="2"
                  ></textarea>

                  <label class="checkbox" *ngIf="field.type === 'checkbox'">
                    <input type="checkbox" [formControlName]="field.key" />
                    Enabled
                  </label>
                </div>
              </div>
            </div>

            <div class="form-actions">
              <button class="btn-base btn-primary" type="submit" [disabled]="form.invalid">Save</button>
              <button class="btn-base btn-secondary" type="button" (click)="resetForm()">Clear</button>
              <button class="btn-base btn-danger" type="button" (click)="deleteSelected()" [disabled]="!selected() || !cfg.deletePath">Delete</button>
            </div>
          </form>
        </aside>
      </div>
    </section>

    <ng-template #missingResource>
      <section class="missing">
        <h2>Entity not found</h2>
        <p>Select a valid module from the sidebar.</p>
        <a routerLink="/dashboard">Go to dashboard</a>
      </section>
    </ng-template>
  `,
  styles: `
    .entity-page {
      display: grid;
      gap: 0.8rem;
      height: 100%;
      min-height: 0;
      grid-template-rows: auto auto 1fr;
    }
    .toolbar {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
      border: 1px solid #d9e4f1;
      background: linear-gradient(150deg, #ffffff, #f4f9ff);
      border-radius: 1rem;
      padding: 0.85rem;
    }
    .toolbar h1 { margin: 0.2rem 0; display: flex; align-items: center; gap: 0.4rem; font-size: 1.25rem; }
    .toolbar p { margin: 0; color: #5f6f82; font-size: 0.9rem; }
    .eyebrow { text-transform: uppercase; font-size: 0.69rem; letter-spacing: 0.09em; color: #0f4c5c !important; font-weight: 700; }
    .toolbar-actions { display: flex; gap: 0.45rem; align-items: center; }
    .toolbar-actions .app-input { width: 200px; border-radius: 999px; }
    .status {
      margin: 0;
      background: #eff9f8;
      color: #0f4c5c;
      border: 1px solid #b8e1d9;
      border-radius: 0.7rem;
      padding: 0.45rem 0.7rem;
      font-size: 0.88rem;
    }
    .content-grid { display: grid; grid-template-columns: 2fr 1fr; gap: 0.8rem; min-height: 0; }
    .table-panel,
    .panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      background: linear-gradient(160deg, #ffffff, #f7fbff);
      min-height: 0;
      display: grid;
      grid-template-rows: auto 1fr;
    }
    .table-header-actions {
      padding: 0.6rem 0.7rem;
      border-bottom: 1px solid #edf1f6;
      display: flex;
      gap: 0.4rem;
      flex-wrap: wrap;
    }
    .table-wrap { overflow: auto; min-height: 0; }
    table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
    th, td { text-align: left; padding: 0.55rem 0.6rem; border-bottom: 1px solid #edf1f6; vertical-align: top; }
    th { position: sticky; top: 0; background: #0f4c5c; color: #ffffff; font-weight: 700; }
    .actions { display: flex; gap: 0.35rem; }
    tr.selected-row { background: #dff1ff; }
    tr:hover { background: #eef6ff; }
    .panel { padding: 0.7rem; }
    .panel h2 { margin: 0 0 0.5rem; font-size: 1.03rem; }
    form { min-height: 0; display: grid; grid-template-rows: 1fr auto; gap: 0.6rem; }
    .form-scroll { min-height: 0; overflow: auto; }
    .form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 0.55rem; }
    .field { display: grid; gap: 0.3rem; }
    .field label { font-size: 0.82rem; font-weight: 700; color: #43576d; }
    .checkbox { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.85rem; }
    .form-actions {
      border-top: 1px solid #edf1f6;
      padding-top: 0.55rem;
      display: flex;
      gap: 0.45rem;
      flex-wrap: wrap;
    }
    .empty { padding: 0.8rem; margin: 0; color: #6d8096; }
    .missing { border: 1px dashed #c9d5e5; border-radius: 1rem; padding: 1rem; background: #fff; }
    @media (max-width: 1200px) {
      .content-grid { grid-template-columns: 1fr; }
      .form-grid { grid-template-columns: 1fr; }
      .entity-page { grid-template-rows: auto auto auto; height: auto; }
    }
    @media (max-width: 760px) {
      .toolbar { flex-direction: column; }
      .toolbar-actions { width: 100%; flex-wrap: wrap; }
      .toolbar-actions .app-input { width: 100%; }
    }
  `
})
export class EntitiesPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);

  readonly config = signal<ResourceConfig | null>(null);
  readonly rows = signal<any[]>([]);
  readonly message = signal('');
  readonly editing = signal(false);
  readonly selected = signal<any | null>(null);
  readonly query = signal('');

  form: FormGroup = this.fb.group({});

  readonly columns = computed(() => {
    const sample = this.rows()[0];
    if (!sample) {
      return this.config()?.fields.map((f) => f.key).slice(0, 8) ?? [];
    }

    return Object.keys(sample)
      .filter((key) => typeof sample[key] !== 'object')
      .slice(0, 8);
  });

  readonly filteredRows = computed(() => {
    const term = this.query().trim().toLowerCase();
    if (!term) {
      return this.rows();
    }

    return this.rows().filter((item) => JSON.stringify(item).toLowerCase().includes(term));
  });

  constructor() {
    this.route.paramMap.subscribe({
      next: (params) => {
        const resource = params.get('resource') ?? '';
        const cfg = RESOURCE_MAP[resource] ?? null;
        this.config.set(cfg);
        this.rebuildForm(cfg);
        this.rows.set([]);
        this.resetForm();

        if (!cfg) {
          return;
        }

        this.api.list(cfg.listPath).subscribe({
          next: (rows) => this.rows.set(rows ?? []),
          error: (err) => this.message.set(this.describeError(err))
        });
      }
    });
  }

  reload(): void {
    const cfg = this.config();
    if (!cfg) {
      return;
    }

    this.api.list(cfg.listPath).subscribe({
      next: (rows) => this.rows.set(rows ?? []),
      error: (err) => this.message.set(this.describeError(err))
    });
  }

  clearSearch(): void {
    this.query.set('');
  }

  startCreate(): void {
    this.editing.set(false);
    this.selected.set(null);
    this.message.set('Create mode');
    this.resetForm();
  }

  selectRow(row: any): void {
    this.selected.set(row);
  }

  isSelected(row: any): boolean {
    return this.selected() === row;
  }

  edit(row: any): void {
    const cfg = this.config();
    if (!cfg || cfg.readOnly) {
      return;
    }

    this.selectRow(row);
    this.editing.set(true);

    cfg.fields.forEach((field) => {
      const rawValue = row[field.key];
      const normalized = this.toFormValue(rawValue, field.type);
      this.form.get(field.key)?.setValue(normalized);
    });

    this.message.set('Edit mode');
  }

  duplicateSelected(): void {
    const cfg = this.config();
    const row = this.selected();
    if (!cfg || !row || cfg.readOnly) {
      return;
    }

    this.editing.set(false);
    cfg.fields.forEach((field) => {
      const rawValue = row[field.key];
      const normalized = this.toFormValue(rawValue, field.type);
      this.form.get(field.key)?.setValue(normalized);
    });

    this.message.set('Selected record copied to form. Click Save to create a new one.');
  }

  save(): void {
    const cfg = this.config();
    if (!cfg || cfg.readOnly) {
      return;
    }

    const payload = this.buildPayload(cfg.fields);

    if (!this.editing()) {
      if (!cfg.createPath) {
        this.message.set('Create endpoint not configured.');
        return;
      }

      this.api.post(cfg.createPath, payload).subscribe({
        next: () => {
          this.message.set(`${cfg.title} created.`);
          this.reload();
          this.resetForm();
        },
        error: (err) => this.message.set(this.describeError(err))
      });

      return;
    }

    if (!cfg.updatePath) {
      this.message.set('Update endpoint not configured.');
      return;
    }

    const selected = this.selected();
    const finalPayload = this.withUpdateIds(cfg, payload, selected);
    const updatePath = this.resolvePath(cfg.updatePath, selected, finalPayload);

    this.api.put(updatePath, finalPayload).subscribe({
      next: () => {
        this.message.set(`${cfg.title} updated.`);
        this.reload();
        this.resetForm();
      },
      error: (err) => this.message.set(this.describeError(err))
    });
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row) {
      return;
    }

    this.remove(row);
  }

  remove(row: any): void {
    const cfg = this.config();
    if (!cfg || cfg.readOnly || !cfg.deletePath) {
      return;
    }

    const deletePath = this.resolvePath(cfg.deletePath, row, row);
    this.api.delete(deletePath).subscribe({
      next: () => {
        this.message.set(`${cfg.title} record removed.`);
        this.reload();
        this.selected.set(null);
      },
      error: (err) => this.message.set(this.describeError(err))
    });
  }

  resetForm(): void {
    const cfg = this.config();
    if (!cfg) {
      return;
    }

    cfg.fields.forEach((field) => {
      const value = field.type === 'checkbox' ? false : '';
      this.form.get(field.key)?.setValue(value);
    });

    this.editing.set(false);
  }

  toDisplay(value: unknown): string {
    if (value === null || value === undefined || value === '') {
      return '-';
    }

    if (typeof value === 'boolean') {
      return value ? 'Yes' : 'No';
    }

    if (typeof value === 'object') {
      return JSON.stringify(value);
    }

    return String(value);
  }

  private rebuildForm(cfg: ResourceConfig | null): void {
    const controls = cfg?.fields ?? [];
    const groupConfig: Record<string, any> = {};

    controls.forEach((field) => {
      const validators = field.required ? [Validators.required] : [];
      groupConfig[field.key] = [field.type === 'checkbox' ? false : '', validators];
    });

    this.form = this.fb.group(groupConfig);
  }

  private resolvePath(path: string, row: any, payload: any): string {
    const values: Record<string, string | number | undefined | null> = {
      id: this.extractId(row),
      roleId: payload?.roleId ?? row?.roleId,
      featureId: payload?.featureId ?? row?.featureId
    };

    return buildPath(path, values);
  }

  private withUpdateIds(cfg: ResourceConfig, payload: any, row: any): any {
    const next = { ...payload };
    if (cfg.idField && row) {
      const fromRow = row[cfg.idField] ?? row.id;
      if (fromRow !== undefined) {
        next[cfg.idField] = fromRow;
      }
    }

    return next;
  }

  private buildPayload(fields: FieldDef[]): Record<string, unknown> {
    const raw = this.form.getRawValue() as Record<string, unknown>;
    const payload: Record<string, unknown> = {};

    fields.forEach((field) => {
      const value = raw[field.key];
      const transformed = this.transformFieldValue(value, field.type);
      if (transformed !== undefined) {
        payload[field.key] = transformed;
      }
    });

    return payload;
  }

  private transformFieldValue(value: unknown, type: FieldDef['type']): unknown {
    if (type === 'checkbox') {
      return Boolean(value);
    }

    if (value === null || value === undefined) {
      return undefined;
    }

    if (type === 'number') {
      if (value === '') {
        return undefined;
      }
      const num = Number(value);
      return Number.isFinite(num) ? num : undefined;
    }

    if (typeof value === 'string') {
      const trimmed = value.trim();
      if (!trimmed) {
        return undefined;
      }

      if (type === 'datetime-local') {
        const date = new Date(trimmed);
        return Number.isNaN(date.getTime()) ? undefined : date.toISOString();
      }

      return trimmed;
    }

    return value;
  }

  private extractId(row: any): any {
    const cfg = this.config();
    if (!cfg || !row) {
      return undefined;
    }

    if (cfg.idField && row[cfg.idField] !== undefined) {
      return row[cfg.idField];
    }

    if (cfg.routeIdParam && row[cfg.routeIdParam] !== undefined) {
      return row[cfg.routeIdParam];
    }

    return row.id;
  }

  private toFormValue(value: any, type: FieldDef['type']): any {
    if (value === null || value === undefined) {
      return type === 'checkbox' ? false : '';
    }

    if (type === 'date' && typeof value === 'string') {
      return value.slice(0, 10);
    }

    if (type === 'datetime-local' && typeof value === 'string') {
      return value.slice(0, 16);
    }

    return value;
  }

  private describeError(err: any): string {
    const fromApi = err?.error?.message ?? err?.error?.title ?? err?.message;
    return fromApi ? `Request failed: ${fromApi}` : 'Request failed';
  }
}
