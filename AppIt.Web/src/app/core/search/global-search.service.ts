import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from '../api/api.service';

export interface GlobalSearchResult {
    type: 'reservation' | 'customer';
    id: number;
    label: string;
    subtitle?: string;
    route: string[];
}

@Injectable({ providedIn: 'root' })
export class GlobalSearchService {
    private readonly api = inject(ApiService);

    search(term: string): Observable<GlobalSearchResult[]> {
        const query = term.trim();
        if (!query) {
            return new Observable((subscriber) => {
                subscriber.next([]);
                subscriber.complete();
            });
        }

        return this.api.listPage('/api/reservations', 1, 10, query).pipe(
            map((page) => (page.items ?? []).map((row: any) => ({
                type: 'reservation' as const,
                id: Number(row.reservationId ?? row.id),
                label: row.reference || row.voucherCode || `Reservation #${row.reservationId ?? row.id}`,
                subtitle: row.customerEmail || row.status,
                route: this.reservationRoute(row)
            })))
        );
    }

    private reservationRoute(row: any): string[] {
        return ['/admin/reservations/booking'];
    }
}
