import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  list(path: string): Observable<any[]> {
    return this.http.get<unknown>(path).pipe(map((res) => this.toArray(this.unwrap(res))));
  }

  get(path: string): Observable<any> {
    return this.http.get<unknown>(path).pipe(map((res) => this.unwrap(res)));
  }

  post(path: string, payload: unknown): Observable<any> {
    return this.http.post<unknown>(path, payload).pipe(map((res) => this.unwrap(res)));
  }

  put(path: string, payload: unknown): Observable<any> {
    return this.http.put<unknown>(path, payload).pipe(map((res) => this.unwrap(res)));
  }

  delete(path: string): Observable<any> {
    return this.http.delete<unknown>(path).pipe(map((res) => this.unwrap(res)));
  }

  private unwrap(value: unknown): any {
    let current: unknown = value;

    while (current !== null && current !== undefined && typeof current === 'object') {
      const obj = current as Record<string, unknown>;
      if ('data' in obj) {
        current = obj['data'];
        continue;
      }

      break;
    }

    return current;
  }

  private toArray(value: unknown): any[] {
    if (Array.isArray(value)) {
      return value;
    }

    if (value === null || value === undefined) {
      return [];
    }

    if (typeof value === 'object') {
      const obj = value as Record<string, unknown>;
      if (Array.isArray(obj['items'])) {
        return obj['items'] as any[];
      }
    }

    return [value];
  }
}
