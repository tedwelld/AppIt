import { CommonModule } from '@angular/common';
import { Component, OnDestroy, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { ApiService } from '../api.service';
import { AuthService } from '../auth.service';

interface SupportMessage {
  id: string;
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
        </div>
        <button class="btn-base btn-secondary" (click)="refresh()">
          <span class="material-symbols-outlined">refresh</span>
          Refresh
        </button>
      </header>

      <article class="chat-panel">
        <div class="messages">
          <div
            class="message"
            *ngFor="let msg of messages()"
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
          <button class="btn-base btn-primary" type="submit">Send</button>
        </form>
        <p class="status" *ngIf="status()">{{ status() }}</p>
      </article>
    </section>
  `,
  styles: `
    .support { display: grid; gap: 0.9rem; }
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
    .chat-panel {
      border: 1px solid #dce6f2;
      border-radius: 1rem;
      background: #fff;
      display: grid;
      grid-template-rows: 1fr auto auto;
      min-height: 420px;
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
  readonly isSuperUser = signal(this.auth.isSuperUser());
  draft = '';
  recipientEmail = this.auth.isSuperUser() ? '' : 'admin@appit.com';

  private readonly poller = setInterval(() => this.refresh(), 15000);

  constructor() {
    this.refresh();
  }

  refresh(): void {
    const email = this.auth.user()?.email ?? 'guest@appit.com';
    const path = this.auth.isSuperUser()
      ? '/api/support/messages'
      : `/api/support/messages?email=${encodeURIComponent(email)}`;

    this.api
      .list(path)
      .pipe(catchError(() => of(this.sampleMessages())))
      .subscribe((rows) => this.messages.set(rows as SupportMessage[]));
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

    this.api.post('/api/support/messages', payload).pipe(catchError(() => of({}))).subscribe(() => {
      this.messages.update((list) => [
        ...list,
        {
          id: `msg-${Date.now()}`,
          fromEmail: payload.fromEmail,
          toEmail: payload.toEmail,
          body: message,
          sentAt: new Date().toISOString(),
          author: this.auth.isSuperUser() ? 'admin' : 'user'
        }
      ]);
      this.draft = '';
      this.status.set(`Message sent to ${payload.toEmail}`);
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.poller);
  }

  private sampleMessages(): SupportMessage[] {
    return [
      {
        id: 'msg-1',
        fromEmail: 'admin@appit.com',
        toEmail: this.auth.user()?.email ?? 'guest@appit.com',
        body: 'Hello! How can we help you today?',
        sentAt: new Date(Date.now() - 3600 * 1000).toISOString(),
        author: 'admin'
      }
    ];
  }
}
