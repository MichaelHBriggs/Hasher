import { RunResults } from "../DataObjects/runResults";
import { AuditableThing } from "../DataObjects/auditableThing";

export interface JobInfo extends AuditableThing {
    name: string;
    rootFolder: string;
    extensions: string[];
    foundFilesCount: number;
    processedFilesCount: number;
    filesHashedCount: number;
    mostRecentRun: RunResults | null;
    percentHashed: number;
    percentProcessed: number;
}
