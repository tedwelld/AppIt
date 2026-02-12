export type FieldType = 'text' | 'number' | 'email' | 'textarea' | 'date' | 'datetime-local' | 'checkbox';

export interface FieldDef {
  key: string;
  label: string;
  type: FieldType;
  required?: boolean;
  placeholder?: string;
}

export interface ResourceConfig {
  key: string;
  title: string;
  icon: string;
  listPath: string;
  createPath?: string;
  getPath?: string;
  updatePath?: string;
  deletePath?: string;
  idField?: string;
  routeIdParam?: string;
  fields: FieldDef[];
  readOnly?: boolean;
  summary?: string;
}

export interface SnapshotSummary {
  id: number;
  reportKey: string;
  title: string;
  snapshotDate: string;
}

export interface SnapshotDetail extends SnapshotSummary {
  dataJson: string;
  generatedByUserId: number;
}

export type CurrencyCode = 'USD' | 'ZAR' | 'GBP';

export interface Product {
  id: string;
  name: string;
  description?: string;
  category: string;
  basePriceUsd: number;
}

export interface Accommodation {
  id: string;
  type: 'Single' | 'Double' | 'Express' | 'Standard';
  description?: string;
  capacity: number;
  basePriceUsd: number;
}

export interface Activity {
  id: string;
  name: string;
  description?: string;
  basePriceUsd: number;
}

export interface Reservation {
  id: string;
  userId: string;
  reference: string;
  voucherCode: string;
  currency: CurrencyCode;
  totalAmount: number;
  status: 'Pending' | 'Confirmed' | 'Cancelled';
  createdAt: string;
}

export interface Invoice {
  id: string;
  reservationId: string;
  totalAmount: number;
  currency: CurrencyCode;
  status: 'Paid' | 'Not Paid' | 'Pending' | 'Cancelled';
  issuedAt: string;
}

export interface Payment {
  id: string;
  invoiceId: string;
  method: 'Mastercard' | 'PayPal' | 'CashApp' | 'EcoCash' | 'Bank Transfer';
  status: 'Paid' | 'Pending' | 'Cancelled';
  transactionReference: string;
  processedAt?: string;
}

export interface Voucher {
  id: string;
  code: string;
  reference: string;
  type: 'Reservation' | 'Accommodation' | 'Activity' | 'Combo';
  createdAt: string;
}
