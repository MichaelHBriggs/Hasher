import { RestService } from "./rest.service";
import { Observable, catchError, retry } from "rxjs";
import { RunResults } from "../DataObjects/runResults";
import { JobInfo } from "../DataObjects/jobInfo";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root',
  })
  export class JobsRestService extends RestService {
    endpoint:string = "Jobs";

    getAllJobs(): Observable<JobInfo[]>{
        return this.get<JobInfo[]>(this.endpoint + "/GetAllJobs");
    }

    getAllJobCount(): Observable<number>{
        return this.get<number>(this.endpoint + "/GetAllJobCount");
    }

    getAllJobsByPage(pageNumber:number, pageSize:number): Observable<JobInfo[]>{
        return  this.get<JobInfo[]>(this.endpoint + "/GetAllJobsByPage/" + pageNumber + "/" + pageSize);
    }

    getJobsForRun(runId:string):Observable<JobInfo[]>{
        return this.get<JobInfo[]>(this.endpoint + "/GetJobsForRun/" + runId);
    }
  }