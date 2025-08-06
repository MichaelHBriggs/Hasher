export interface AuditableThing {
  id: string;
  createdAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
  isDeleted: boolean;
  durationInSeconds: number;
}
