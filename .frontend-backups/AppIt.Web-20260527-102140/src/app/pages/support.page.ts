import { CommonModule } from '@angular/common';
import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { ApiService } from '../api.service';
import { AuthService } from '../auth.service';

interface SupportMessage {
  id: number | string;
  fromEmail: string;
  toEmail: string;
  body: string;
  sentAt: string;
  author: 'user' | 'admin';
}

@Component({
  selector: 'app-support-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="support">
      <header class="hero">
        <div>
          <p class="eyebrow">Support Chat</p>
          <h1>Live assistance via admin@appit.com</h1>
          <p class="sub">Messages are delivered to the admin inbox and synced here.</p>
          <input
            class="app-input search-input"
            [ngModel]="searchTerm()"
            (ngModelChange)="searchTerm.set($event)"
            placeholder="Search messages..."
          />
        </div>
        <button class="btn-base btn-secondary btn-compact" (click)="refresh()">
          <span class="material-symbols-outlined">refresh</span>
          Refresh
        </button>
      </header>

      <article class="chat-panel">
        <div class="messages">
          <div
            class="message"
            *ngFor="let msg of filteredMessages()"
            [class.outgoing]="msg.author === 'user'"
          >
            <div class="meta">
              <span>{{ msg.author === 'user' ? 'You' : 'Admin' }}</span>
              <span>{{ msg.sentAt | date: 'short' }}</span>
            </div>
            <p>{{ msg.body }}</p>
          </div>
        </div>

        <form class="composer" (ngSubmit)="send()">
          <input
            *ngIf="isSuperUser()"
            class="app-input"
            placeholder="Recipient email"
            [(ngModel)]="recipientEmail"
            name="recipientEmail"
          />
          <input
            class="app-input"
            placeholder="Type your message"
            [(ngModel)]="draft"
            name="draft"
          />
          <button class="btn-base btn-primary btn-compact" type="submit">Send</button>
        </form>
        <p class="status" *ngIf="status()">{{ status() }}</p>
      </article>
    </section>
  `,
  styles: `
    .support { display: grid; gap: 0.7rem; height: 100%; min-height: 0; grid-template-rows: auto 1fr; }
    .hero {
      border: 1px solid #dbe3ef;
      border-radius: 1rem;
      padding: 0.8rem;
      background: linear-gradient(150deg, #ffffff, #f4f9ff);
      display: flex;
      justify-content: space-between;
      gap: 1rem;
    }
    .eyebrow { margin: 0; text-transform: uppercase; letter-spacing: 0.12em; font-size: 0.7rem; color: #0f4c5c; font-weight: 700; }
    .sub { margin: 0; color: #5b6f85; font-size: 0.85rem; }
    .search-input { margin-top: 0.55rem; width: min(340px, 100%); }
    .chat-panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      background: #fff;
      display: grid;
      grid-template-rows: 1fr auto auto;
      min-height: 0;
      height: 100%;
      overflow: hidden;
    }
    .messages {
      padding: 0.8rem;
      overflow: auto;
      display: grid;
      gap: 0.6rem;
      background: linear-gradient(160deg, #ffffff, #f7fbff);
    }
    .message {
      border: 1px solid #dce6f2;
      border-radius: 0.9rem;
      padding: 0.6rem;
      max-width: 70%;
      background: #fff;
    }
    .message.outgoing {
      margin-left: auto;
      background: #0f4c5c;
      color: #fff;
      border-color: #0f4c5c;
    }
    .message p { margin: 0.2rem 0 0; }
    .meta {
      display: flex;
      justify-content: space-between;
      font-size: 0.7rem;
      opacity: 0.8;
    }
    .composer {
      padding: 0.6rem;
      border-top: 1px solid #e1e8f1;
      display: grid;
      grid-template-columns: 1fr 1.2fr auto;
      gap: 0.5rem;
      position: sticky;
      bottom: 0;
      background: #fff;
      z-index: 2;
    }
    .hero .btn-base,
    .composer .btn-base {
      min-width: 6.6rem;
    }
    .status { margin: 0; padding: 0.5rem 0.7rem; font-size: 0.8rem; color: #0f4c5c; background: #eaf7f5; border-top: 1px solid #cce8e3; }
    @media (max-width: 760px) {
      .hero { flex-direction: column; }
      .message { max-width: 100%; }
      .composer { grid-template-columns: 1fr; }
    }
  `
})
export class SupportPageComponent implements OnDestroy {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  readonly messages = signal<SupportMessage[]>([]);
  readonly status = signal('');
  readonly searchTerm = signal('');
  readonly isSuperUser = signal(this.auth.isSuperUser());
  readonly filteredMessages = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    if (!query) {
      return this.messages();
    }

    return this.messages().filter((message) =>
      `${message.fromEmail} ${message.toEmail} ${message.body}`.toLowerCase().includes(query)
    );
  });
  draft = '';
  recipientEmail = this.auth.isSuperUser() ? '' : 'admin@appit.com';

  private pollDelayMs = 15000;
  private poller: ReturnType<typeof setTimeout> | undefined;
  private lastRefreshFailed = false;

  constructor() {
    this.refresh();
  }

  refresh(): void {
    const path = this.auth.isSuperUser()
      ? '/api/support/messages'
      : '/api/support/messages/mine';

    this.api
      .list(path)
      .pipe(catchError((err) => {
        this.status.set(this.describeError(err));
        this.lastRefreshFailed = true;
        this.increaseBackoff();
        this.scheduleNextPoll();
        return of([]);
      }))
      .subscribe((rows) => {
        if (this.lastRefreshFailed) {
          this.lastRefreshFailed = false;
          return;
        }

        const email = this.auth.user()?.email ?? 'guest@appit.com';
        this.messages.set(this.normalizeMessages(rows as any[], email));
        this.resetBackoff();
        this.scheduleNextPoll();
      });
  }

  send(): void {
    const message = this.draft.trim();
    if (!message) {
      return;
    }

    const fromEmail = this.auth.user()?.email ?? 'guest@appit.com';
    const toEmail = this.auth.isSuperUser()
      ? this.recipientEmail.trim() || 'guest@appit.com'
      : 'admin@appit.com';

    const payload = {
      fromEmail,
      toEmail,
      message
    };

    this.api.post('/api/support/messages', payload).subscribe({
      next: () => {
        this.refresh();
        this.draft = '';
        this.status.set(`Message sent to ${payload.toEmail}`);
      },
      error: (err) => {
        this.status.set(this.describeError(err));
      }
    });
  }

  ngOnDestroy(): void {
    if (this.poller) {
      clearTimeout(this.poller);
    }
  }

  private scheduleNextPoll(): void {
    if (this.poller) {
      clearTimeout(this.poller);
    }

    this.poller = setTimeout(() => this.refresh(), this.pollDelayMs);
  }

  private resetBackoff(): void {
    this.pollDelayMs = 15000;
  }

  private increaseBackoff(): void {
    this.pollDelayMs = Math.min(this.pollDelayMs * 2, 60000);
  }

  private normalizeMessages(rows: any[], userEmail: string): SupportMessage[] {
    return (rows ?? []).map((row, index) => {
      const fromEmail = String(row?.fromEmail ?? 'admin@appit.com');
      const toEmail = String(row?.toEmail ?? userEmail);
      const body = String(row?.body ?? row?.message ?? '');
      const createdAt = String(row?.sentAt ?? row?.createdAt ?? new Date().toISOString());

      return {
        id: row?.id ?? `msg-${index}`,
        fromEmail,
        toEmail,
        body,
        sentAt: createdAt,
        author: fromEmail.toLowerCase() === 'admin@appit.com' ? 'admin' : 'user'
      } as SupportMessage;
    });
  }

  private describeError(err: any): string {
    const fromApi = err?.error?.error?.detail ?? err?.error?.error?.title ?? err?.error?.message ?? err?.message;
    return fromApi ? `Support sync failed: ${fromApi}` : 'Support sync failed.';
  }
}
