export type AppItRole = 'regular' | 'admin' | 'super';

export interface ApiEnvelope<T> {
    success?: boolean;
    data?: T;
    message?: string;
    time?: string;
}

export interface AppItUser {
    id: number | string;
    role: AppItRole | string;
    roleId?: number;
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    avatarUrl?: string;
    preferredCurrency?: string;
    isActive?: boolean;
}

export interface AuthResponse {
    user: AppItUser;
    token?: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    preferredCurrency?: string;
    password: string;
}

export interface PasswordResetRequestResponse {
    message?: string;
    resetToken?: string;
}

export interface ListResponse<T = any> {
    items?: T[];
    totalCount?: number;
    page?: number;
    pageSize?: number;
    totalPages?: number;
}

export interface EntityConfig {
    key: string;
    title: string;
    endpoint: string;
    icon: string;
    group?: string;
    idFields?: string[];
    readonly?: boolean;
}

export interface AdminStats {
    totalAccounts?: number;
    totalReservations?: number;
    totalInvoices?: number;
    totalPayments?: number;
    totalRevenue?: number;
    totalSales?: number;
    totalBookings?: number;
    totalEarnings?: number;
    totalCustomers?: number;
    pendingPayments?: number;
    activeVouchers?: number;
    [key: string]: unknown;
}

export interface Customer {
    id?: number;
    firstName?: string;
    surname?: string;
    email?: string;
    phoneNumber?: string;
    nationality?: string;
    region?: string;
    durationOfStayDays?: number;
    notes?: string;
    agentCompanyId?: number | null;
}

export interface BookingServiceItem {
    id?: number;
    serviceType: 'Product' | 'Accommodation' | 'Activity';
    serviceId: number;
    serviceName: string;
    quantity: number;
    unitPrice: number;
    totalPrice?: number;
    currency: string;
}

export interface BookingCheckoutRequest {
    customerId?: number | null;
    tripAccountId?: number | null;
    customer?: Customer | null;
    reservation: {
        accountId?: number | string | null;
        customerId?: number | null;
        agencyId?: number | null;
        currency: string;
        totalAmount: number;
        status: string;
        customerEmail?: string | null;
    };
    invoice: {
        totalAmount: number;
        currency: string;
        status: string;
    };
    payment: {
        method: string;
        amount: number;
        currencyCode: string;
        idempotencyKey?: string;
    };
    serviceItems: BookingServiceItem[];
}

export interface BookingCheckoutResult {
    customer?: Customer;
    reservation?: Record<string, any>;
    invoice?: Record<string, any>;
    payment?: Record<string, any>;
    voucher?: Record<string, any>;
    serviceItems?: BookingServiceItem[];
}
