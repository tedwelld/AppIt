import { MenuItem } from 'primeng/api';
import { AppItRole, EntityConfig } from '../api/api.models';

export const APPIT_ENTITIES: EntityConfig[] = [
    { key: 'accounts', title: 'Accounts', endpoint: '/api/accounts', icon: 'pi pi-users', group: 'Access', idFields: ['id'] },
    { key: 'roles', title: 'Roles', endpoint: '/api/roles', icon: 'pi pi-shield', group: 'Access', idFields: ['id', 'roleId'] },
    { key: 'features', title: 'Features', endpoint: '/api/features', icon: 'pi pi-sitemap', group: 'Access', idFields: ['id'] },
    { key: 'permissions', title: 'Permissions', endpoint: '/api/permissions', icon: 'pi pi-key', group: 'Access', idFields: ['permissionId', 'id'] },
    { key: 'departments', title: 'Departments', endpoint: '/api/departments', icon: 'pi pi-briefcase', group: 'Access', idFields: ['id'] },
    { key: 'trip-accounts', title: 'Trip Accounts', endpoint: '/api/trip-accounts', icon: 'pi pi-users', group: 'Reservations', idFields: ['tripAccountId', 'id'] },
    { key: 'companies', title: 'Companies', endpoint: '/api/companies', icon: 'pi pi-building-columns', group: 'Setup', idFields: ['companyId', 'id'] },
    { key: 'suppliers', title: 'Suppliers', endpoint: '/api/suppliers', icon: 'pi pi-truck', group: 'Setup', idFields: ['supplierId', 'id'] },
    { key: 'products', title: 'Products', endpoint: '/api/products', icon: 'pi pi-box', group: 'Catalog', idFields: ['productId', 'id'] },
    { key: 'currencies', title: 'Currencies', endpoint: '/api/currencies', icon: 'pi pi-money-bill', group: 'Setup', idFields: ['id'] },
    { key: 'service-prices', title: 'Service Prices', endpoint: '/api/service-prices', icon: 'pi pi-dollar', group: 'Catalog', idFields: ['id'] },
    { key: 'reservations', title: 'Reservations', endpoint: '/api/reservations', icon: 'pi pi-calendar', group: 'Bookings', idFields: ['id', 'reservationId'] },
    { key: 'invoices', title: 'Invoices', endpoint: '/api/invoices', icon: 'pi pi-receipt', group: 'Finance', idFields: ['id', 'invoiceId'] },
    { key: 'payments', title: 'Payments', endpoint: '/api/payments', icon: 'pi pi-credit-card', group: 'Finance', idFields: ['id', 'paymentId'] },
    { key: 'vouchers', title: 'Vouchers', endpoint: '/api/vouchers', icon: 'pi pi-ticket', group: 'Finance', idFields: ['id', 'voucherId'] },
    { key: 'support-messages', title: 'Support Messages', endpoint: '/api/support/messages', icon: 'pi pi-comments', group: 'Service', idFields: ['id', 'messageId'] },
    { key: 'credit-notes', title: 'Credit Notes', endpoint: '/api/credit-notes', icon: 'pi pi-undo', group: 'Finance', idFields: ['id'] },
    { key: 'refunds', title: 'Refunds', endpoint: '/api/refunds', icon: 'pi pi-refresh', group: 'Finance', idFields: ['id'] },
    { key: 'proof-of-payments', title: 'Proof Of Payments', endpoint: '/api/proof-of-payments', icon: 'pi pi-credit-card', group: 'Finance', idFields: ['id'] },
    { key: 'commissions', title: 'Commissions', endpoint: '/api/commissions', icon: 'pi pi-percentage', group: 'Finance', idFields: ['id'] },
    { key: 'consultants', title: 'Consultants', endpoint: '/api/consultants', icon: 'pi pi-user-plus', group: 'Setup', idFields: ['id'] },
    { key: 'product-categories', title: 'Product Categories', endpoint: '/api/product-categories', icon: 'pi pi-tag', group: 'Catalog', idFields: ['id'] },
    { key: 'product-sub-categories', title: 'Product Sub-Categories', endpoint: '/api/product-sub-categories', icon: 'pi pi-tags', group: 'Catalog', idFields: ['id'] },
    { key: 'special-product-prices', title: 'Special Prices', endpoint: '/api/special-product-prices', icon: 'pi pi-star', group: 'Catalog', idFields: ['id'] }
];

export interface WorkspaceMenuModel {
    home: MenuItem | null;
    groups: MenuItem[];
}

interface WorkspaceMenuItem extends MenuItem {
    feature?: string;
    permissions?: string[];
}

const ALL_FEATURES = '*';

/**
 * Feature areas each named role may access. Keys are lowercased backend role names.
 * '*' grants every feature. Roles not listed fall back to the admin-tier default
 * (everything except the super-only 'System' area).
 */
export const ROLE_FEATURES: Record<string, string[] | typeof ALL_FEATURES> = {
    'super': ALL_FEATURES,
    'admin': ALL_FEATURES,
    'general manager': ALL_FEATURES,
    'finance director': ['Accounts', 'Cashier', 'Statistics', 'Reservations', 'System'],
    'accountant': ['Accounts', 'Cashier', 'Statistics', 'System'],
    'accounts clerk': ['Accounts', 'System'],
    'cashier': ['Cashier', 'Accounts', 'System'],
    'operations manager': ['Operations', 'Reservations', 'Statistics', 'System'],
    'reservation manager/supervisor': ['Reservations', 'Operations', 'Statistics', 'System'],
    'coordinator': ['Reservations', 'Operations', 'System'],
    'driver guide': ['Operations', 'System'],
    'consultant/trainee': ['Reservations', 'System'],
    'marketing director': ['Statistics', 'Reservations', 'System'],
    'it devs': ['Setup', 'Administration', 'System'],
    'it engineer/manager': ['Setup', 'Administration', 'System']
};

function allowedFeaturesFor(role: AppItRole | null, roleName?: string | null): string[] | typeof ALL_FEATURES {
    const key = (roleName ?? '').trim().toLowerCase();
    if (key && ROLE_FEATURES[key]) return ROLE_FEATURES[key];
    if (role === 'super') return ALL_FEATURES;
    // Admin-tier default: everything except the super-only System area.
    return ['Reservations', 'Cashier', 'Accounts', 'Statistics', 'Operations', 'Setup', 'Administration'];
}

function action(label: string, icon: string, routerLink: unknown[], feature: string, permissions: string[] = []): WorkspaceMenuItem {
    return { label, icon, routerLink, feature, permissions };
}

function canShow(item: WorkspaceMenuItem, allowed: string[] | typeof ALL_FEATURES): boolean {
    if (!item.feature || item.feature === 'User') return true;
    if (allowed === ALL_FEATURES) return true;
    return allowed.includes(item.feature);
}

function filterMenu(items: WorkspaceMenuItem[], allowed: string[] | typeof ALL_FEATURES): WorkspaceMenuItem[] {
    return items
        .map((item) => ({
            ...item,
            items: item.items ? filterMenu(item.items as WorkspaceMenuItem[], allowed) : undefined
        }))
        .filter((item) => canShow(item, allowed) && (!item.items || item.items.length > 0));
}

export function buildWorkspaceMenu(role: AppItRole | null, roleName?: string | null): WorkspaceMenuModel {
    if (role === 'admin' || role === 'super') {
        const allowed = allowedFeaturesFor(role, roleName);
        const groups: WorkspaceMenuItem[] = [
            {
                label: 'Reservations',
                icon: 'pi pi-fw pi-bookmark-fill',
                feature: 'Reservations',
                items: [
                    action('Booking Capture', 'pi pi-fw pi-book', ['/admin/reservations/booking'], 'Reservations', ['New Booking', 'Edit or Confirm']),
                    action('Trip Accounts', 'pi pi-fw pi-users', ['/admin/reservations/groups'], 'Reservations', ['Add Agent', 'Edit Agent', 'Delete Agent']),
                    action('Reports', 'pi pi-fw pi-file', ['/admin/reservations/reports'], 'Reservations', ['View Trip Accounts']),
                    action('Availability Calendar', 'pi pi-fw pi-calendar', ['/admin/reservations/availability-calendar'], 'Reservations', ['New Booking', 'Edit or Confirm']),
                    action('Occupancy Calendar', 'pi pi-fw pi-calendar-clock', ['/admin/reservations/occupancy-calendar'], 'Reservations', ['New Booking', 'Edit or Confirm']),
                    action('Flow Charts', 'pi pi-fw pi-chart-line', ['/admin/reservations/flow-charts'], 'Reservations', ['Print or View Charts'])
                ]
            },
            {
                label: 'Cashier',
                icon: 'pi pi-fw pi-money-bill',
                feature: 'Cashier',
                items: [
                    action('Exchange Rates', 'pi pi-fw pi-dollar', ['/admin/cashier/exchange-rates'], 'Cashier', ['Exchange Rate (Change All)', 'Agent Exchange Rate']),
                    action('Bank Note Details', 'pi pi-fw pi-wallet', ['/admin/cashier/bank-note-details'], 'Cashier', ['BankNote Details']),
                    action('Cashier Reports', 'pi pi-fw pi-file-export', ['/admin/cashier/reports'], 'Cashier', ['Cashier Reports'])
                ]
            },
            {
                label: 'Accounts',
                icon: 'pi pi-fw pi-id-card',
                feature: 'Accounts',
                items: [
                    action('Invoicing', 'pi pi-fw pi-receipt', ['/admin/accounts/invoicing'], 'Accounts', ['Search for Invoice (USD)', 'Create Statement']),
                    action('Credit Notes', 'pi pi-fw pi-undo', ['/admin/accounts/credit-notes'], 'Accounts', ['Create Credit Notes and Refund']),
                    action('Refunds', 'pi pi-fw pi-refresh', ['/admin/accounts/refunds'], 'Accounts', ['Process Refund']),
                    action('Commissions', 'pi pi-fw pi-percentage', ['/admin/accounts/commissions'], 'Accounts', ['View Commissions']),
                    action('Deposit Reports', 'pi pi-fw pi-file-check', ['/admin/accounts/deposit-reports'], 'Accounts', ['Deposit Reports']),
                    action('Proof Of Payment', 'pi pi-fw pi-credit-card', ['/admin/accounts/proof-of-payments'], 'Accounts', ['Proof Of Payment'])
                ]
            },
            {
                label: 'Statistics',
                icon: 'pi pi-fw pi-chart-bar',
                feature: 'Statistics',
                items: [
                    action('All Reports', 'pi pi-fw pi-table', ['/admin/statistics/all-reports'], 'Statistics', ['By Department', 'By Product', 'By Region', 'Key Stats']),
                    action('Executive Stats', 'pi pi-fw pi-chart-line', ['/admin/statistics/executive-stats'], 'Statistics', ['Executive Stats'])
                ]
            },
            {
                label: 'Operations',
                icon: 'pi pi-fw pi-check-square',
                feature: 'Operations',
                items: [
                    action('Check In', 'pi pi-fw pi-check-circle', ['/admin/operations/check-in'], 'Operations', ['Print or View Travelled PAX']),
                    action('Day-End Audit', 'pi pi-fw pi-sun', ['/admin/operations/day-end'], 'Operations', ['Open Day', 'Close Day']),
                    action('Opera Management', 'pi pi-fw pi-sync', ['/admin/operations/opera-management'], 'Operations', ['Print or View Product Capacity'])
                ]
            },
            {
                label: 'Setup',
                icon: 'pi pi-fw pi-th-large',
                feature: 'Setup',
                items: [
                    action('Manage Companies', 'pi pi-fw pi-building', ['/admin/setup/manage-companies'], 'Setup', ['Add Company', 'Edit Company']),
                    action('Manage Products', 'pi pi-fw pi-shopping-cart', ['/admin/setup/manage-products'], 'Setup', ['Add Product', 'Edit Product']),
                    action('Service Prices', 'pi pi-fw pi-dollar', ['/admin/setup/manage-service-prices'], 'Setup', ['Add Product', 'Edit Product']),
                    action('Manage Departments', 'pi pi-fw pi-briefcase', ['/admin/setup/manage-departments'], 'Setup', ['Edit Departments']),
                    action('Manage Currencies', 'pi pi-fw pi-money-bill', ['/admin/setup/manage-currencies'], 'Setup', ['Add Currency', 'Edit Currency']),
                    action('Manage Suppliers', 'pi pi-fw pi-truck', ['/admin/setup/manage-suppliers'], 'Setup', ['Add Supplier', 'Edit Supplier']),
                    action('Manage Consultants', 'pi pi-fw pi-user-plus', ['/admin/setup/manage-consultants'], 'Setup', ['Add Consultant', 'Edit Consultant']),
                    action('Product Categories', 'pi pi-fw pi-tag', ['/admin/setup/manage-product-categories'], 'Setup', ['Add Category', 'Edit Category']),
                    action('Product Sub-Categories', 'pi pi-fw pi-tags', ['/admin/setup/manage-product-sub-categories'], 'Setup', ['Add Sub-Category', 'Edit Sub-Category']),
                    action('Special Prices', 'pi pi-fw pi-star', ['/admin/setup/manage-special-prices'], 'Setup', ['Add Special Price', 'Edit Special Price']),
                    action('Manage Features', 'pi pi-fw pi-sitemap', ['/admin/setup/manage-features'], 'Setup', ['Edit Features']),
                    action('Manage Permissions', 'pi pi-fw pi-key', ['/admin/setup/manage-permissions'], 'Setup', ['Edit Permissions'])
                ]
            },
            {
                label: 'Administration',
                icon: 'pi pi-fw pi-lock',
                feature: 'Administration',
                items: [
                    action('Manage Users', 'pi pi-fw pi-users', ['/admin/administration/users'], 'Administration', ['Activate User', 'Export Data']),
                    action('Manage Roles', 'pi pi-fw pi-shield', ['/admin/administration/roles'], 'Administration', ['Edit Roles', 'Edit Role Permissions']),
                    action('User Activity Log', 'pi pi-fw pi-history', ['/admin/administration/user-activity'], 'Administration', ['System Log'])
                ]
            },
            {
                label: 'System',
                icon: 'pi pi-fw pi-cog',
                feature: 'System',
                items: [
                    action('Support Inbox', 'pi pi-fw pi-comments', ['/admin/support'], 'System'),
                    action('Settings', 'pi pi-fw pi-cog', ['/admin/settings'], 'System')
                ]
            }
        ];

        return {
            home: { label: 'Dashboard', icon: 'pi pi-fw pi-chart-line', routerLink: ['/admin/dashboard'] },
            groups: filterMenu(groups, allowed)
        };
    }

    return {
        home: { label: 'My Dashboard', icon: 'pi pi-fw pi-home', routerLink: ['/user/dashboard'] },
        groups: [
            {
                label: 'My AppIt',
                icon: 'pi pi-fw pi-user',
                items: [
                    { label: 'New Booking', icon: 'pi pi-fw pi-plus-circle', routerLink: ['/user/bookings/new'] },
                    { label: 'Support', icon: 'pi pi-fw pi-comments', routerLink: ['/user/support'] },
                    { label: 'Settings', icon: 'pi pi-fw pi-cog', routerLink: ['/user/settings'] }
                ]
            }
        ]
    };
}
