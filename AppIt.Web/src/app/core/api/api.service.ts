import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { EMPTY, expand, map, Observable, reduce } from 'rxjs';
import { API_BASE_URL } from './api.constants';
import {
    ApiEnvelope,
    AppItUser,
    AuthResponse,
    BookingCheckoutRequest,
    BookingCheckoutResult,
    Customer,
    ListResponse,
    LoginRequest,
    PasswordResetRequestResponse,
    RegisterRequest
} from './api.models';

export const SYSTEM_PAGE_SIZE = 10;

@Injectable({ providedIn: 'root' })
export class ApiService {
    private readonly http = inject(HttpClient);

    login(request: LoginRequest): Observable<AuthResponse> {
        return this.http.post<unknown>(`${API_BASE_URL}/auth/login`, request).pipe(map((res) => this.unwrap<AuthResponse>(res)));
    }

    register(request: RegisterRequest): Observable<AuthResponse> {
        return this.http.post<unknown>(`${API_BASE_URL}/auth/register`, request).pipe(map((res) => this.unwrap<AuthResponse>(res)));
    }

    requestPasswordReset(email: string): Observable<PasswordResetRequestResponse> {
        return this.http
            .post<unknown>(`${API_BASE_URL}/auth/password-reset/request`, { email })
            .pipe(map((res) => this.unwrap<PasswordResetRequestResponse>(res)));
    }

    confirmPasswordReset(email: string, token: string, newPassword: string): Observable<unknown> {
        return this.http.post<unknown>(`${API_BASE_URL}/auth/password-reset/confirm`, { email, token, newPassword });
    }

    list<T = any>(path: string): Observable<T[]> {
        return this.http.get<unknown>(this.url(path)).pipe(map((res) => this.toArray<T>(this.unwrap<unknown>(res))));
    }

    /**
     * Loads every record for a reference list by paging through the API, so dropdowns
     * are never silently truncated to the default page size. Use for small/medium
     * lookups (currencies, suppliers, consultants, trip accounts).
     */
    listAll<T = any>(path: string, pageSize = 200): Observable<T[]> {
        return this.listPage<T>(path, 1, pageSize).pipe(
            expand((res) => {
                const page = res.page ?? 1;
                const total = res.totalCount ?? (res.items?.length ?? 0);
                return page * pageSize < total ? this.listPage<T>(path, page + 1, pageSize) : EMPTY;
            }),
            reduce((acc: T[], res) => acc.concat(res.items ?? []), [] as T[])
        );
    }

    /** Returns the true total record count for a resource (not capped by page size). */
    count(path: string): Observable<number> {
        return this.listPage(path, 1, 1).pipe(map((page) => page.totalCount ?? (page.items?.length ?? 0)));
    }

    listPage<T = any>(path: string, page = 1, pageSize = SYSTEM_PAGE_SIZE, search = ''): Observable<ListResponse<T>> {
        const separator = path.includes('?') ? '&' : '?';
        const query = `page=${page}&pageSize=${pageSize}${search.trim() ? `&search=${encodeURIComponent(search.trim())}` : ''}`;
        return this.http.get<unknown>(this.url(`${path}${separator}${query}`)).pipe(map((res) => this.toPage<T>(this.unwrap<unknown>(res))));
    }

    listMinePage<T = any>(path: string, accountId: number | string, page = 1, pageSize = SYSTEM_PAGE_SIZE, search = ''): Observable<ListResponse<T>> {
        const separator = path.includes('?') ? '&' : '?';
        const query = `accountId=${encodeURIComponent(String(accountId))}&page=${page}&pageSize=${pageSize}${search.trim() ? `&search=${encodeURIComponent(search.trim())}` : ''}`;
        return this.http.get<unknown>(this.url(`${path}${separator}${query}`)).pipe(map((res) => this.toPage<T>(this.unwrap<unknown>(res))));
    }

    get<T = any>(path: string): Observable<T> {
        return this.http.get<unknown>(this.url(path)).pipe(map((res) => this.unwrap<T>(res)));
    }

    post<T = any>(path: string, payload: unknown): Observable<T> {
        return this.http.post<unknown>(this.url(path), payload).pipe(map((res) => this.unwrap<T>(res)));
    }

    postBlob(path: string, payload: unknown): Observable<Blob> {
        return this.http.post(this.url(path), payload, { responseType: 'blob' });
    }

    put<T = any>(path: string, payload: unknown): Observable<T> {
        return this.http.put<unknown>(this.url(path), payload).pipe(map((res) => this.unwrap<T>(res)));
    }

    delete<T = any>(path: string): Observable<T> {
        return this.http.delete<unknown>(this.url(path)).pipe(map((res) => this.unwrap<T>(res)));
    }

    searchCustomers(search = ''): Observable<Customer[]> {
        return this.listPage<Customer>('/api/customers', 1, SYSTEM_PAGE_SIZE, search).pipe(map((page) => page.items ?? []));
    }

    getPricingQuote(serviceType: string, serviceId: number, currency: string, consultantId?: number | null): Observable<{ unitPrice: number; exchangeRate: number }> {
        const params = new URLSearchParams({
            serviceType,
            serviceId: String(serviceId),
            currency
        });
        if (consultantId) {
            params.set('consultantId', String(consultantId));
        }
        return this.get(`/api/pricing/quote?${params.toString()}`);
    }

    listNotifications(): Observable<any[]> {
        return this.list('/api/notifications');
    }

    markNotificationRead(id: number): Observable<unknown> {
        return this.put(`/api/notifications/${id}/read`, {});
    }

    checkoutBooking(request: BookingCheckoutRequest): Observable<BookingCheckoutResult> {
        return this.post<BookingCheckoutResult>('/api/bookings/checkout', request);
    }

    private url(path: string): string {
        return path.startsWith('/api') ? path : `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
    }

    private unwrap<T>(value: unknown): T {
        if (value && typeof value === 'object' && 'data' in value) {
            return (value as ApiEnvelope<T>).data as T;
        }

        return value as T;
    }

    private toArray<T>(value: unknown): T[] {
        if (Array.isArray(value)) {
            return value as T[];
        }

        const list = value as { data?: ListResponse<T> | T[]; items?: T[] };
        if (Array.isArray(list?.items)) {
            return list.items;
        }

        if (Array.isArray(list?.data)) {
            return list.data;
        }

        if (list?.data && !Array.isArray(list.data) && Array.isArray((list.data as ListResponse<T>).items)) {
            return (list.data as ListResponse<T>).items ?? [];
        }

        return [];
    }

    private toPage<T>(value: unknown): ListResponse<T> {
        if (Array.isArray(value)) {
            return { items: value as T[], totalCount: value.length, page: 1, pageSize: SYSTEM_PAGE_SIZE, totalPages: 1 };
        }

        const page = value as ListResponse<T>;
        if (Array.isArray(page?.items)) {
            return page;
        }

        const nested = value as { data?: ListResponse<T> | T[] };
        if (Array.isArray(nested?.data)) {
            return { items: nested.data as T[], totalCount: nested.data.length, page: 1, pageSize: SYSTEM_PAGE_SIZE, totalPages: 1 };
        }

        if (nested?.data && Array.isArray((nested.data as ListResponse<T>).items)) {
            return nested.data as ListResponse<T>;
        }

        return { items: [], totalCount: 0, page: 1, pageSize: SYSTEM_PAGE_SIZE, totalPages: 0 };
    }
}
