import { ResourceConfig } from './models';

export const RESOURCE_CONFIGS: ResourceConfig[] = [
  {
    key: 'accounts',
    title: 'Accounts',
    icon: 'badge',
    summary: 'User identities and role assignment.',
    listPath: '/api/Accounts',
    createPath: '/api/Accounts',
    getPath: '/api/Accounts/{id}',
    updatePath: '/api/Accounts/{id}',
    deletePath: '/api/Accounts/{id}',
    routeIdParam: 'id',
    idField: 'id',
    fields: [
      { key: 'firstName', label: 'First Name', type: 'text', required: true },
      { key: 'lastName', label: 'Last Name', type: 'text', required: true },
      { key: 'nationalId', label: 'National ID', type: 'text', required: true },
      { key: 'email', label: 'Email', type: 'email', required: true },
      { key: 'roleId', label: 'Role ID', type: 'number', required: true },
      { key: 'isActive', label: 'Is Active', type: 'checkbox' }
    ]
  },
  {
    key: 'roles',
    title: 'Roles',
    icon: 'admin_panel_settings',
    summary: 'Access role setup.',
    listPath: '/api/Roles',
    createPath: '/api/Roles',
    getPath: '/api/Roles/{id}',
    updatePath: '/api/Roles/{id}',
    deletePath: '/api/Roles/{id}',
    idField: 'roleId',
    routeIdParam: 'id',
    fields: [{ key: 'name', label: 'Role Name', type: 'text', required: true }]
  },
  {
    key: 'products',
    title: 'Products',
    icon: 'inventory_2',
    summary: 'Products and pricing.',
    listPath: '/api/Product',
    createPath: '/api/Product',
    getPath: '/api/Product/{id}',
    updatePath: '/api/Product/{id}',
    deletePath: '/api/Product/{id}',
    idField: 'productId',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'price', label: 'Price', type: 'number', required: true }
    ]
  },
  {
    key: 'companies',
    title: 'Companies',
    icon: 'apartment',
    summary: 'Corporate profile records.',
    listPath: '/api/Company',
    createPath: '/api/Company',
    getPath: '/api/Company/{id}',
    updatePath: '/api/Company/{id}',
    deletePath: '/api/Company/{id}',
    idField: 'companyId',
    routeIdParam: 'id',
    fields: [
      { key: 'companyName', label: 'Company Name', type: 'text', required: true },
      { key: 'companyAddress', label: 'Address', type: 'text' },
      { key: 'companyEmail', label: 'Email', type: 'email' },
      { key: 'companyPhone', label: 'Phone', type: 'text' },
      { key: 'regNumber', label: 'Registration Number', type: 'text' },
      { key: 'accountNumber', label: 'Account Number', type: 'text' },
      { key: 'vatNumber', label: 'VAT Number', type: 'text' }
    ]
  },
  {
    key: 'customers',
    title: 'Customers',
    icon: 'groups',
    summary: 'Customer identities and profile data.',
    listPath: '/api/Customer',
    createPath: '/api/Customer',
    getPath: '/api/Customer/{id}',
    updatePath: '/api/Customer/{id}',
    deletePath: '/api/Customer/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'title', label: 'Title', type: 'text' },
      { key: 'firstName', label: 'First Name', type: 'text', required: true },
      { key: 'surname', label: 'Surname', type: 'text', required: true },
      { key: 'idType', label: 'ID Type', type: 'text' },
      { key: 'nationality', label: 'Nationality', type: 'text' },
      { key: 'dob', label: 'Date of Birth', type: 'date' },
      { key: 'profession', label: 'Profession', type: 'text' },
      { key: 'proxyName', label: 'Proxy Name', type: 'text' },
      { key: 'email', label: 'Email', type: 'email' },
      { key: 'phoneNumber', label: 'Phone', type: 'text' },
      { key: 'taxCategory', label: 'Tax Category', type: 'text' },
      { key: 'region', label: 'Region', type: 'text' },
      { key: 'durationOfStayDays', label: 'Duration of Stay (Days)', type: 'number' },
      { key: 'lastSavedBy', label: 'Last Saved By (User ID)', type: 'number' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
      { key: 'customerTypeId', label: 'Customer Type ID', type: 'number' }
    ]
  },
  {
    key: 'customer-types',
    title: 'Customer Types',
    icon: 'category',
    summary: 'Taxation and grouping metadata.',
    listPath: '/api/CustomerType',
    createPath: '/api/CustomerType',
    getPath: '/api/CustomerType/{id}',
    updatePath: '/api/CustomerType/{id}',
    deletePath: '/api/CustomerType/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'customerId', label: 'Customer ID', type: 'number', required: true },
      { key: 'family', label: 'Family', type: 'number' },
      { key: 'customerGroup', label: 'Customer Group', type: 'number' },
      { key: 'groupNumber', label: 'Group Number', type: 'number' },
      { key: 'taxationPercentage', label: 'Taxation %', type: 'number' },
      { key: 'age', label: 'Age', type: 'number' },
      { key: 'specialPrice', label: 'Special Price', type: 'text' },
      { key: 'disability', label: 'Disability', type: 'number' },
      { key: 'lastSavedBy', label: 'Last Saved By (User ID)', type: 'number' },
      { key: 'notes', label: 'Notes', type: 'textarea' }
    ]
  },
  {
    key: 'departments',
    title: 'Departments',
    icon: 'corporate_fare',
    summary: 'Operational teams and ownership.',
    listPath: '/api/Department',
    createPath: '/api/Department',
    getPath: '/api/Department/{id}',
    updatePath: '/api/Department/{id}',
    deletePath: '/api/Department/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Department Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'updatedBy', label: 'Updated By (User ID)', type: 'number', required: true }
    ]
  },
  {
    key: 'suppliers',
    title: 'Suppliers',
    icon: 'local_shipping',
    summary: 'Vendor and supplier records.',
    listPath: '/api/Suppliers',
    createPath: '/api/Suppliers',
    getPath: '/api/Suppliers/{id}',
    updatePath: '/api/Suppliers/{id}',
    deletePath: '/api/Suppliers/{id}',
    idField: 'supplierId',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Supplier Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'contactEmail', label: 'Email', type: 'email' },
      { key: 'contactPhone', label: 'Phone', type: 'text' }
    ]
  },
  {
    key: 'permissions',
    title: 'Permissions',
    icon: 'key',
    summary: 'Permission definitions.',
    listPath: '/api/Permission',
    createPath: '/api/Permission',
    getPath: '/api/Permission/{id}',
    updatePath: '/api/Permission/{id}',
    deletePath: '/api/Permission/{id}',
    idField: 'permissionId',
    routeIdParam: 'id',
    fields: [{ key: 'name', label: 'Permission Name', type: 'text', required: true }]
  },
  {
    key: 'features',
    title: 'Features',
    icon: 'widgets',
    summary: 'Feature catalog and defaults.',
    listPath: '/api/Feature',
    createPath: '/api/Feature',
    getPath: '/api/Feature/{id}',
    updatePath: '/api/Feature/{id}',
    deletePath: '/api/Feature/{id}',
    idField: 'id',
    routeIdParam: 'id',
    fields: [
      { key: 'name', label: 'Feature Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'permissionId', label: 'Permission ID', type: 'number' }
    ]
  },
  {
    key: 'feature-permissions',
    title: 'Feature Permissions',
    icon: 'shield',
    summary: 'Maps features to permissions.',
    listPath: '/api/FeaturePermission',
    createPath: '/api/FeaturePermission',
    getPath: '/api/FeaturePermission/{id}',
    updatePath: '/api/FeaturePermission/{id}',
    deletePath: '/api/FeaturePermission/{id}',
    idField: 'featurePermissionId',
    routeIdParam: 'id',
    fields: [
      { key: 'featureId', label: 'Feature ID', type: 'number', required: true },
      { key: 'permissionId', label: 'Permission ID', type: 'number', required: true }
    ]
  },
  {
    key: 'reservations',
    title: 'Reservations',
    icon: 'event_available',
    summary: 'Reservation lifecycle and invoicing flag.',
    listPath: '/api/Reservation',
    createPath: '/api/Reservation',
    getPath: '/api/Reservation/{id}',
    updatePath: '/api/Reservation/{id}',
    deletePath: '/api/Reservation/{id}',
    idField: 'reservationId',
    routeIdParam: 'id',
    fields: [
      { key: 'customerFirstName', label: 'Customer First Name', type: 'text' },
      { key: 'customerLastName', label: 'Customer Last Name', type: 'text' },
      { key: 'customerIdNumber', label: 'Customer ID Number', type: 'text' },
      { key: 'agencyId', label: 'Agency ID', type: 'number' },
      { key: 'agencyConsultantId', label: 'Agency Consultant ID', type: 'number' },
      { key: 'agencyVoucherReference', label: 'Agency Voucher Ref', type: 'text' },
      { key: 'numberOfPeople', label: 'Number Of People', type: 'number' },
      { key: 'currencyId', label: 'Currency ID', type: 'number' },
      { key: 'currencyExchangeRate', label: 'Exchange Rate', type: 'number' },
      { key: 'country', label: 'Country', type: 'text' },
      { key: 'vat', label: 'VAT', type: 'number' },
      { key: 'isInvoiced', label: 'Is Invoiced', type: 'checkbox' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
      { key: 'analysisId', label: 'Analysis ID', type: 'number' },
      { key: 'customerEmail', label: 'Customer Email', type: 'email' },
      { key: 'closingByUserId', label: 'Closing By User ID', type: 'number' },
      { key: 'closingByUserName', label: 'Closing By User Name', type: 'text' },
      { key: 'closingDate', label: 'Closing Date', type: 'datetime-local' }
    ]
  },
  {
    key: 'invoices',
    title: 'Invoices',
    icon: 'receipt_long',
    summary: 'Create and review invoice totals.',
    listPath: '/api/invoices',
    createPath: '/api/invoices',
    readOnly: false,
    fields: [
      { key: 'reservationId', label: 'Reservation ID', type: 'number', required: true },
      { key: 'totalAmount', label: 'Total Amount', type: 'number', required: true }
    ]
  },
  {
    key: 'role-features',
    title: 'Role Features',
    icon: 'tune',
    summary: 'Role to feature activation map.',
    listPath: '/api/RoleFeatures',
    createPath: '/api/RoleFeatures',
    updatePath: '/api/RoleFeatures/{roleId}/{featureId}',
    deletePath: '/api/RoleFeatures/{roleId}/{featureId}',
    idField: 'roleFeatureId',
    fields: [
      { key: 'roleId', label: 'Role ID', type: 'number', required: true },
      { key: 'featureId', label: 'Feature ID', type: 'number', required: true },
      { key: 'isActivated', label: 'Is Activated', type: 'checkbox' }
    ]
  },
  {
    key: 'audit-logs',
    title: 'Audit Logs',
    icon: 'history',
    summary: 'Read-only audit trail.',
    listPath: '/api/audit-logs',
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
