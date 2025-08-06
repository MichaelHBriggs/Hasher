import { AuditableThing } from "../DataObjects/auditableThing";

export interface RunResults extends AuditableThing {
    addedFiles: number;
    updatedFiles: number;
    deletedFiles: number;
    unchangedFiles: number;
    totalFiles: number;
    isActive: boolean;
}
