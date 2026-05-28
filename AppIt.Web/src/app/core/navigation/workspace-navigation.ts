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
    { key: 'accommodations', title: 'Accommodations', endpoint: '/api/accommodations', icon: 'pi pi-building', group: 'Catalog', idFields: ['accommodationId', 'id'] },
    { key: 'currencies', title: 'Currencies', endpoint: '/api/currencies', icon: 'pi pi-money-bill', group: 'Setup', idFields: ['id'] },
    { key: 'activities', title: 'Activities', endpoint: '/api/activities', icon: 'pi pi-compass', group: 'Catalog', idFields: ['activityId', 'id'] },
    { key: 'reservations', title: 'Reservations', endpoint: '/api/reservations', icon: 'pi pi-calendar', group: 'Bookings', idFields: ['id', 'reservationId'] },
    { key: 'invoices', title: 'Invoices', endpoint: '/api/invoices', icon: 'pi pi-receipt', group: 'Finance', idFields: ['id', 'invoiceId'] },
    { key: 'payments', title: 'Payments', endpoint: '/api/payments', icon: 'pi pi-credit-card', group: 'Finance', idFields: ['id', 'paymentId'] },
    { key: 'vouchers', title: 'Vouchers', endpoint: '/api/vouchers', icon: 'pi pi-ticket', group: 'Finance', idFields: ['id', 'voucherId'] },
    { key: 'support-messages', title: 'Support Messages', endpoint: '/api/support/messages', icon: 'pi pi-comments', group: 'Service', idFields: ['id', 'messageId'] }
];

export interface WorkspaceMenuModel {
    home: MenuItem | null;
    groups: MenuItem[];
}

interface WorkspaceMenuItem extends MenuItem {
    feature?: string;
    permissions?: string[];
}

function action(label: string, icon: string, routerLink: unknown[], feature: string, permissions: string[] = []): WorkspaceMenuItem {
    return { label, icon, routerLink, feature, permissions };
}

function canShow(item: WorkspaceMenuItem, role: AppItRole | null): boolean {
    if (role === 'super') return true;
    if (role === 'admin') return item.feature !== 'System';
    return !item.feature || item.feature === 'User';
}

function filterMenu(items: WorkspaceMenuItem[], role: AppItRole | null): WorkspaceMenuItem[] {
    return items
        .map((item) => ({
            ...item,
            items: item.items ? filterMenu(item.items as WorkspaceMenuItem[], role) : undefined
        }))
        .filter((item) => canShow(item, role) && (!item.items || item.items.length > 0));
}

export function buildWorkspaceMenu(role: AppItRole | null): WorkspaceMenuModel {
    if (role === 'admin' || role === 'super') {
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
                    action('Manage Accommodations', 'pi pi-fw pi-building', ['/admin/setup/manage-accommodations'], 'Setup', ['Add Product', 'Edit Product']),
                    action('Manage Activities', 'pi pi-fw pi-compass', ['/admin/setup/manage-activities'], 'Setup', ['Add Product', 'Edit Product']),
                    action('Manage Departments', 'pi pi-fw pi-briefcase', ['/admin/setup/manage-departments'], 'Setup', ['Edit Departments']),
                    action('Manage Currencies', 'pi pi-fw pi-money-bill', ['/admin/setup/manage-currencies'], 'Setup', ['Add Currency', 'Edit Currency']),
                    action('Manage Suppliers', 'pi pi-fw pi-truck', ['/admin/setup/manage-suppliers'], 'Setup', ['Add Supplier', 'Edit Supplier']),
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
            groups: filterMenu(groups, role)
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
