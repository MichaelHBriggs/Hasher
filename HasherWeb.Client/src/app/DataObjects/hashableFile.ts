import { RunResults } from "../DataObjects/runResults";
import { JobInfo } from "../DataObjects/jobInfo";
import { AuditableThing } from "../DataObjects/auditableThing";

export interface HashableFile extends AuditableThing {
    name: string;
    size: number;
    hash: string;
    lastModified: string;
    lastRun: RunResults | null;
    lastJob: JobInfo | null;
    extension: string;
}
