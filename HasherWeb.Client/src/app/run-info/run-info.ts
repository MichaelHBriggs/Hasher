import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RunInfoDetails } from '../run-info-details/run-info-details';
import { RunResults } from '../DataObjects/runResults';
import { Observable } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { RunRestService } from "../RestServices/runs.rest.service"

@Component({
  selector: 'app-run-info',
  imports: [ CommonModule, RunInfoDetails ],
  template: `<section>
  <form>
    <button class="primary pill" type="button" (click)="getAllRunResults()" >Get all runs</button>
  </form>
</section>
<section class="results">
  <app-run-info-details *ngFor="let runResult of runResultsList" [runResult]="runResult"></app-run-info-details>
</section>`,
  styleUrl: './run-info.css'
})
export class RunInfo {
  runResultsList:RunResults[] =[];


  constructor( private api:RunRestService){}

  getAllRunResults()  {
    console.log("getAllResults()");
      this.api.getAllRuns().subscribe(data => {
          for (const r of data){
            this.runResultsList.push(r);
          }
          console.log(this.runResultsList);
      });
  }
}
