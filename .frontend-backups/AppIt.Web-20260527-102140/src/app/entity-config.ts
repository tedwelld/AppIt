import { ResourceConfig } from './models';

export const RESOURCE_CONFIGS: ResourceConfig[] = [
  {
    key: 'accounts',
    title: 'Accounts',
    icon: 'badge',
    summary: 'Customer and staff identities.',
    listPath: '/api/accounts',
    createPath: '/api/accounts',
    getPath: '/api/accounts/{id}',
    updatePath: '/api/accounts/{id}',
    deletePath: '/api/accounts/{id}',
    routeIdParam: 'id',
    idField: 'id',
    fields: [
      { key: 'firstName', label: 'First Name', type: 'text', required: true },
      { key: 'lastName', label: 'Last Name', type: 'text', required: true },
      { key: 'email', label: 'Email', type: 'email', required: true },
      { key: 'phone', label: 'Phone', type: 'text' },
      { key: 'role', label: 'Role (regular/super)', type: 'text', required: true },
      { key: 'preferredCurrency', label: 'Preferred Currency', type: 'text' },
      { key: 'avatarUrl', label: 'Avatar URL', type: 'text' },
      { key: 'isActive', label: 'Is Active', type: 'checkbox' }
    ]
  },
  {
    key: 'roles',
    title: 'Roles',
    icon: 'admin_panel_settings',
    summary: 'Access role setup.',
    listPath: '/api/roles',
    createPath: '/api/roles',
    getPath: '/api/roles/{id}',
    updatePath: '/api/roles/{id}',
    deletePath: '/api/roles/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Role Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' }
    ]
  },
  {
    key: 'products',
    title: 'Products',
    icon: 'inventory_2',
    summary: 'Adventure products and pricing.',
    listPath: '/api/products',
    createPath: '/api/products',
    getPath: '/api/products/{id}',
    updatePath: '/api/products/{id}',
    deletePath: '/api/products/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'category', label: 'Category', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'basePriceUsd', label: 'Base Price (USD)', type: 'number', required: true },
      { key: 'isActive', label: 'Is Active', type: 'checkbox' }
    ]
  },
  {
    key: 'accommodations',
    title: 'Accommodations',
    icon: 'hotel',
    summary: 'Room types and nightly rates.',
    listPath: '/api/accommodations',
    createPath: '/api/accommodations',
    getPath: '/api/accommodations/{id}',
    updatePath: '/api/accommodations/{id}',
    deletePath: '/api/accommodations/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'type', label: 'Type', type: 'text', required: true },
      { key: 'capacity', label: 'Capacity', type: 'number', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'basePriceUsd', label: 'Base Price (USD)', type: 'number', required: true },
      { key: 'isActive', label: 'Is Active', type: 'checkbox' }
    ]
  },
  {
    key: 'activities',
    title: 'Activities',
    icon: 'paragliding',
    summary: 'Adventure activities catalog.',
    listPath: '/api/activities',
    createPath: '/api/activities',
    getPath: '/api/activities/{id}',
    updatePath: '/api/activities/{id}',
    deletePath: '/api/activities/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'basePriceUsd', label: 'Base Price (USD)', type: 'number', required: true },
      { key: 'isActive', label: 'Is Active', type: 'checkbox' }
    ]
  },
  {
    key: 'reservations',
    title: 'Reservations',
    icon: 'event_available',
    summary: 'Bookings, vouchers, and references.',
    listPath: '/api/reservations',
    createPath: '/api/reservations',
    getPath: '/api/reservations/{id}',
    updatePath: '/api/reservations/{id}',
    deletePath: '/api/reservations/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'reference', label: 'Reference', type: 'text', required: true },
      { key: 'voucherCode', label: 'Voucher Code', type: 'text', required: true },
      { key: 'customerId', label: 'Customer ID', type: 'text', required: true },
      { key: 'currency', label: 'Currency', type: 'text', required: true },
      { key: 'totalAmount', label: 'Total Amount', type: 'number', required: true },
      { key: 'status', label: 'Status', type: 'text', required: true },
      { key: 'createdAt', label: 'Created At', type: 'datetime-local' }
    ]
  },
  {
    key: 'invoices',
    title: 'Invoices',
    icon: 'receipt_long',
    summary: 'Invoice totals and payment status.',
    listPath: '/api/invoices',
    createPath: '/api/invoices',
    getPath: '/api/invoices/{id}',
    updatePath: '/api/invoices/{id}',
    deletePath: '/api/invoices/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'reservationId', label: 'Reservation ID', type: 'text', required: true },
      { key: 'totalAmount', label: 'Total Amount', type: 'number', required: true },
      { key: 'currency', label: 'Currency', type: 'text', required: true },
      { key: 'status', label: 'Status', type: 'text', required: true },
      { key: 'issuedAt', label: 'Issued At', type: 'datetime-local' }
    ]
  },
  {
    key: 'payments',
    title: 'Payments',
    icon: 'payments',
    summary: 'Payment status and gateways.',
    listPath: '/api/payments',
    createPath: '/api/payments',
    getPath: '/api/payments/{id}',
    updatePath: '/api/payments/{id}',
    deletePath: '/api/payments/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'invoiceId', label: 'Invoice ID', type: 'text', required: true },
      { key: 'method', label: 'Method', type: 'text', required: true },
      { key: 'status', label: 'Status', type: 'text', required: true },
      { key: 'transactionReference', label: 'Transaction Reference', type: 'text', required: true },
      { key: 'processedAt', label: 'Processed At', type: 'datetime-local' }
    ]
  },
  {
    key: 'vouchers',
    title: 'Vouchers',
    icon: 'confirmation_number',
    summary: 'Generated voucher references.',
    listPath: '/api/vouchers',
    createPath: '/api/vouchers',
    getPath: '/api/vouchers/{id}',
    updatePath: '/api/vouchers/{id}',
    deletePath: '/api/vouchers/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'code', label: 'Voucher Code', type: 'text', required: true },
      { key: 'reference', label: 'Reference', type: 'text', required: true },
      { key: 'type', label: 'Type', type: 'text', required: true },
      { key: 'comboReference', label: 'Combo Reference', type: 'text' },
      { key: 'createdAt', label: 'Created At', type: 'datetime-local' }
    ]
  },
  {
    key: 'support-messages',
    title: 'Support Messages',
    icon: 'support_agent',
    summary: 'Inbound assistance requests.',
    listPath: '/api/support/messages',
    createPath: '/api/support/messages',
    getPath: '/api/support/messages/{id}',
    updatePath: '/api/support/messages/{id}',
    deletePath: '/api/support/messages/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'fromEmail', label: 'From Email', type: 'email', required: true },
      { key: 'subject', label: 'Subject', type: 'text', required: true },
      { key: 'message', label: 'Message', type: 'textarea', required: true },
      { key: 'status', label: 'Status', type: 'text' },
      { key: 'createdAt', label: 'Created At', type: 'datetime-local' }
    ]
  },
  {
    key: 'report-snapshots',
    title: 'Report Snapshots',
    icon: 'analytics',
    summary: 'Generated report snapshots (read-only).',
    listPath: '/api/report-snapshots',
    readOnly: true,
    fields: []
  }
];

export const RESOURCE_MAP = RESOURCE_CONFIGS.reduce<Record<string, ResourceConfig>>((acc, current) => {
  acc[current.key] = current;
  return acc;
}, {});

export function buildPath(
  template: string,
  values: Record<string, string | number | undefined | null>
): string {
  return template.replace(/\{(.*?)\}/g, (_, token: string) => {
    const value = values[token];
    return value === undefined || value === null ? '' : encodeURIComponent(String(value));
  });
}
