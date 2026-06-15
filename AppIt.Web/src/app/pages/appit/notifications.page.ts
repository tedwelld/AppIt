import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService } from '../../core/api/api.service';

@Component({
    standalone: true,
    imports: [CommonModule, ButtonModule],
    template: `
        <section class="grid gap-5">
            <div class="workspace-card flex items-center justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">Notifications</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">Inbox</h1>
                    <p class="text-muted-color m-0">{{ unreadCount() }} unread</p>
                </div>
                <button pButton type="button" icon="pi pi-refresh" label="Refresh" (click)="load()"></button>
            </div>

            <article class="workspace-card grid gap-3">
                <div *ngIf="loading()" class="text-center py-8"><i class="pi pi-spin pi-spinner text-3xl text-primary"></i></div>
                <div *ngFor="let item of notifications()" class="p-4 rounded-xl border border-surface-200 dark:border-surface-700" [class.opacity-70]="item.isRead">
                    <div class="flex items-start justify-between gap-3">
                        <div>
                            <p class="font-semibold m-0">{{ item.title }}</p>
                            <p class="text-muted-color text-sm mt-1 mb-0">{{ item.message }}</p>
                        </div>
                        <button *ngIf="!item.isRead" pButton type="button" icon="pi pi-check" label="Mark read" class="p-button-sm" (click)="markRead(item.id)"></button>
                    </div>
                </div>
                <p *ngIf="!loading() && !notifications().length" class="text-muted-color text-center py-6 m-0">No notifications yet.</p>
            </article>
            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `
})
export class NotificationsPage {
    private readonly api = inject(ApiService);
    readonly notifications = signal<any[]>([]);
    readonly loading = signal(false);
    readonly status = signal('');
    readonly unreadCount = computed(() => this.notifications().filter((n) => !n.isRead).length);

    constructor() {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.api.listNotifications().subscribe({
            next: (rows) => {
                this.notifications.set(rows ?? []);
                this.loading.set(false);
            },
            error: (err) => {
                this.status.set(extractApiErrorMessage(err?.error ?? err, 'Failed to load notifications.'));
                this.loading.set(false);
            }
        });
    }

    markRead(id: number): void {
        this.api.markNotificationRead(id).subscribe({
            next: () => this.load(),
            error: (err) => this.status.set(extractApiErrorMessage(err?.error ?? err, 'Failed to mark notification read.'))
        });
    }
}
