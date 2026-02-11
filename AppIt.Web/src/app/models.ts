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
