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
    trend?: Array<{ label?: string; value?: number }>;
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
    serviceType: 'Product' | 'Accommodation' | 'Activity' | 'Transfer' | 'Tour' | 'Combo';
    serviceId: number;
    serviceName: string;
    productKind?: string;
    comboId?: number;
    miscCode?: string;
    quantity: number;
    unitPrice: number;
    totalPrice?: number;
    currency: string;
    supplierId?: number | null;
    adultPax?: number | null;
    childPax?: number | null;
    compPax?: number | null;
    rooms?: number | null;
    nights?: number | null;
    pickupLocation?: string | null;
    dropoffLocation?: string | null;
    activityDate?: string | null;
    discountPercent?: number | null;
    vatPercent?: number | null;
    costOfSale?: number | null;
    notes?: string | null;
}

export interface BookingCheckoutRequest {
    customerId?: number | null;
    tripAccountId?: number | null;
    customer?: Customer | null;
    reservation: {
        reference?: string | null;
        voucherCode?: string | null;
        accountId?: number | string | null;
        customerId?: number | null;
        agencyId?: number | null;
        agencyConsultantId?: number | null;
        agencyVoucherReference?: string | null;
        currency: string;
        totalAmount: number;
        status: string;
        customerEmail?: string | null;
        country?: string | null;
        notes?: string | null;
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
        transactionReference?: string;
    };
    proofOfPaymentUrl?: string;
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
