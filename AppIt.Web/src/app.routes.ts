import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { adminGuard, authGuard } from './app/core/auth/auth.guard';
import { AuthPage } from './app/pages/appit/auth.page';
import { AccessPage } from './app/pages/appit/access.page';
import { NotFoundPage } from './app/pages/appit/not-found.page';

export const appRoutes: Routes = [
    { path: '', pathMatch: 'full', redirectTo: 'auth/login' },
    { path: 'auth', pathMatch: 'full', redirectTo: 'auth/login' },
    { path: 'auth/login', component: AuthPage },
    { path: 'access', component: AccessPage },
    {
        path: 'user',
        component: AppLayout,
        canActivate: [authGuard],
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
            { path: 'dashboard', loadComponent: () => import('./app/pages/appit/user-dashboard.page').then((m) => m.UserDashboardPage) },
            { path: 'bookings/new', loadComponent: () => import('./app/pages/appit/booking-wizard.page').then((m) => m.BookingWizardPage) },
            { path: 'support', loadComponent: () => import('./app/pages/appit/support.page').then((m) => m.SupportPage) },
            { path: 'settings', loadComponent: () => import('./app/pages/appit/settings.page').then((m) => m.SettingsPage) }
        ]
    },
    {
        path: 'admin',
        component: AppLayout,
        canActivate: [adminGuard],
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
            { path: 'dashboard', loadComponent: () => import('./app/pages/appit/admin-dashboard.page').then((m) => m.AdminDashboardPage) },
            { path: 'bookings/new', loadComponent: () => import('./app/pages/appit/booking-wizard.page').then((m) => m.BookingWizardPage) },
            { path: 'entities/:resource', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage) },
            { path: 'reports', loadComponent: () => import('./app/pages/appit/reports.page').then((m) => m.ReportsPage) },
            { path: 'reservations/booking', loadComponent: () => import('./app/pages/appit/booking-wizard.page').then((m) => m.BookingWizardPage) },
            { path: 'reservations/groups', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'trip-accounts' } },
            { path: 'reservations/reports', loadComponent: () => import('./app/pages/appit/reports.page').then((m) => m.ReportsPage), data: { reportGroup: 'Reservations' } },
            { path: 'reservations/availability-calendar', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'availability-calendar' } },
            { path: 'reservations/occupancy-calendar', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'occupancy-calendar' } },
            { path: 'reservations/flow-charts', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'flow-charts' } },
            { path: 'cashier/exchange-rates', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'exchange-rates' } },
            { path: 'cashier/bank-note-details', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'bank-note-details' } },
            { path: 'cashier/reports', loadComponent: () => import('./app/pages/appit/reports.page').then((m) => m.ReportsPage), data: { reportGroup: 'Cashier' } },
            { path: 'accounts/invoicing', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'invoicing' } },
            { path: 'accounts/credit-notes', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'credit-notes' } },
            { path: 'accounts/deposit-reports', loadComponent: () => import('./app/pages/appit/reports.page').then((m) => m.ReportsPage), data: { reportGroup: 'Accounts' } },
            { path: 'accounts/proof-of-payments', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'proof-of-payments' } },
            { path: 'statistics/all-reports', loadComponent: () => import('./app/pages/appit/reports.page').then((m) => m.ReportsPage) },
            { path: 'statistics/executive-stats', loadComponent: () => import('./app/pages/appit/admin-dashboard.page').then((m) => m.AdminDashboardPage) },
            { path: 'operations/check-in', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'check-in' } },
            { path: 'operations/opera-management', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'opera-management' } },
            { path: 'setup/manage-companies', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'companies' } },
            { path: 'setup/manage-products', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'products' } },
            { path: 'setup/manage-service-prices', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'service-prices' } },
            { path: 'setup/manage-departments', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'departments' } },
            { path: 'setup/manage-suppliers', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'suppliers' } },
            { path: 'setup/manage-currencies', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'currencies' } },
            { path: 'setup/manage-features', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'features' } },
            { path: 'setup/manage-permissions', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'permissions' } },
            { path: 'administration/users', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'accounts' } },
            { path: 'administration/roles', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'roles' } },
            { path: 'administration/user-activity', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'user-activity' } },
            { path: 'accounts/commissions', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'commissions' } },
            { path: 'accounts/refunds', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'refunds' } },
            { path: 'operations/day-end', loadComponent: () => import('./app/pages/appit/operational-flow.page').then((m) => m.OperationalFlowPage), data: { flowKey: 'day-end' } },
            { path: 'setup/manage-consultants', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'consultants' } },
            { path: 'setup/manage-product-categories', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'product-categories' } },
            { path: 'setup/manage-product-sub-categories', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'product-sub-categories' } },
            { path: 'setup/manage-special-prices', loadComponent: () => import('./app/pages/appit/entities.page').then((m) => m.EntitiesPage), data: { resource: 'special-product-prices' } },
            { path: 'support', loadComponent: () => import('./app/pages/appit/support.page').then((m) => m.SupportPage) },
            { path: 'settings', loadComponent: () => import('./app/pages/appit/settings.page').then((m) => m.SettingsPage) }
        ]
    },
    { path: 'notfound', component: NotFoundPage },
    { path: '**', redirectTo: '/notfound' }
];
