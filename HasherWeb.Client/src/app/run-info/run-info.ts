import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RunInfoDetails } from '../run-info-details/run-info-details';
import { RunResults } from '../DataObjects/runResults';
import { Observable } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { RunRestService } from "../RestServices/runs.rest.service"
import { MatGridListModule } from '@angular/material/grid-list';
import { LogInfo } from "../log-info/log-info"

@Component({
  selector: 'app-run-info',
  imports: [ CommonModule, RunInfoDetails, MatGridListModule, LogInfo ],
  template: `
   <div class="full-width-container">
   <mat-grid-list cols="2" >
      <mat-grid-tile *ngFor="let runResult of runResultsList">
        <app-run-info-details [runResult]="runResult"></app-run-info-details>
      </mat-grid-tile>
  </mat-grid-list>
  </div>
  <div class="main">
      <app-log-info class="full-width-container" ></app-log-info>
  </div>
  `,
  styleUrl: './run-info.css'
})
export class RunInfo {
  runResultsList:RunResults[] =[];

  constructor( private api:RunRestService){}

  ngOnInit(): void {
    console.log("ngOnInit()");
    this.getAllRunResults();
  }


  getAllRunResults()  {
    console.log("getAllResults()");
      this.api.getAllRuns().subscribe(data => {
        if (false){
          for (const r of data){
            this.runResultsList.push(r);
          }
          //console.log(this.runResultsList);
        } else {
          this.runResultsList.push(data[0]);
        }
      });
  }
}
