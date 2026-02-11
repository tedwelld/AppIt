import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class Accounts {
  private readonly api = inject(ApiService);

  getAll() {
    return this.api.list('/api/Accounts');
  }
}
