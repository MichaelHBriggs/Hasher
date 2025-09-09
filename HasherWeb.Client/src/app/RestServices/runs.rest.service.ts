import { RestService } from "./rest.service";
import { Observable, catchError, retry } from "rxjs";
import { RunResults } from "../DataObjects/runResults";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root',
  })
export class RunRestService extends RestService {
    endpoint:string = "Run";


    getActiveRun(): Observable<RunResults> {
        return this.get<RunResults>(this.endpoint + "/GetActiveRun").pipe(retry(3));
    }

    getIsAnyRunActive(): Observable<boolean> {
        return this.get<boolean>(this.endpoint + "/IsAnyRunActie").pipe(retry(3));
    }
    
    getAllRuns(): Observable<RunResults[]> {
        return  this.get<RunResults[]>(this.endpoint + "/GetAllRuns").pipe(retry(3));
    }

    getSpecificRun(id:string): Observable<RunResults> {
        return this.get<RunResults>(this.endpoint + "/GetSpecificRun/" + id).pipe(retry(3));
    }
}
