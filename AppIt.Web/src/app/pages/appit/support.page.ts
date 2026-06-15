import { CommonModule } from '@angular/common';
import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { extractApiErrorMessage } from '../../core/api/api-error';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
        <section class="grid gap-5 h-full">
            <div class="workspace-card flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                <div>
                    <p class="text-primary font-bold uppercase tracking-[0.25em]">Support</p>
                    <h1 class="font-display text-4xl mt-2 mb-0">{{ auth.isAdmin() ? 'Support Inbox' : 'Support Chat' }}</h1>
                </div>
                <button class="p-button p-component p-button-secondary" type="button" (click)="refresh()"><span class="p-button-label">Refresh</span></button>
            </div>

            <article class="workspace-card grid gap-4">
                <div class="grid gap-3 max-h-[55vh] overflow-auto">
                    <div *ngFor="let message of filteredMessages()" class="p-4 rounded-xl border border-surface-200 dark:border-surface-700" [class.ml-auto]="message.fromEmail === currentEmail()" [class.bg-primary-50]="message.fromEmail === currentEmail()">
                        <div class="flex items-center justify-between gap-4 text-xs text-muted-color">
                            <span>{{ message.fromEmail }}</span>
                            <span>{{ message.createdAt || message.sentAt | date:'short' }}</span>
                        </div>
                        <p class="m-0 mt-2">{{ message.body || message.message }}</p>
                    </div>
                </div>

                <form class="grid md:grid-cols-[1fr_2fr_auto] gap-3" (ngSubmit)="send()">
                    <input class="p-inputtext p-component" *ngIf="auth.isAdmin()" [(ngModel)]="toEmail" name="toEmail" placeholder="Recipient email" />
                    <input class="p-inputtext p-component" [(ngModel)]="draft" name="draft" placeholder="Type your message" />
                    <button class="p-button p-component" type="submit"><span class="p-button-label">Send</span></button>
                </form>
            </article>
            <p class="text-red-500" *ngIf="status()">{{ status() }}</p>
        </section>
    `
})
export class SupportPage implements OnDestroy {
    private readonly api = inject(ApiService);
    readonly auth = inject(AuthService);
    readonly messages = signal<any[]>([]);
    readonly status = signal('');
    readonly filteredMessages = computed(() => this.messages());
    draft = '';
    toEmail = '';
    private pollDelayMs = 15000;
    private poller: ReturnType<typeof setTimeout> | undefined;
    private failed = false;

    constructor() {
        this.refresh();
    }

    currentEmail(): string {
        return this.auth.user()?.email ?? 'guest@appit.com';
    }

    refresh(): void {
        const path = this.auth.isAdmin() ? '/api/support/messages' : '/api/support/messages/mine';
        this.api.list(path).pipe(catchError((err) => {
            this.failed = true;
            this.status.set(this.describeError(err));
            this.pollDelayMs = Math.min(this.pollDelayMs * 2, 60000);
            this.schedule();
            return of([]);
        })).subscribe((rows) => {
            this.messages.set(rows);
            if (!this.failed) {
                this.pollDelayMs = 15000;
            }
            this.failed = false;
            this.schedule();
        });
    }

    send(): void {
        const message = this.draft.trim();
        if (!message) return;
        const payload = {
            fromEmail: this.currentEmail(),
            toEmail: this.auth.isAdmin() ? this.toEmail.trim() || 'guest@appit.com' : 'admin@appit.com',
            message
        };
        this.api.post('/api/support/messages', payload).subscribe({
            next: () => {
                this.draft = '';
                this.refresh();
            },
            error: (err) => this.status.set(this.describeError(err))
        });
    }

    ngOnDestroy(): void {
        if (this.poller) clearTimeout(this.poller);
    }

    private schedule(): void {
        if (this.poller) clearTimeout(this.poller);
        this.poller = setTimeout(() => this.refresh(), this.pollDelayMs);
    }

    private describeError(err: any): string {
        return extractApiErrorMessage(err, 'Support sync failed.');
    }
}
