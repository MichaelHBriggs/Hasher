import { Component, Input, signal  } from '@angular/core';
import { MatGridListModule } from '@angular/material/grid-list';
import { RunResults } from "../DataObjects/runResults"
import { CommonModule, formatDate } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { JobDetails } from "../job-details/job-details"
import { SecondsToHhMmSsPipe } from '../seconds-to-hh-mm-ss-pipe';

@Component({
  selector: 'app-run-info-details',
  imports: [ CommonModule, MatTableModule, JobDetails, SecondsToHhMmSsPipe],
  template: `
   <div  id="{{runResult.id}}" [class]="hostClass">
      <table >
        <tr><th>Started at:</th><td>{{ runResult.createdAt | date: 'M/d/yy HH:mm:ss' }} GMT</td></tr>
        <tr><th>Duration: </th><td> {{ runResult.durationInSeconds | secondsToHhMmSsPipe }}</td></tr>
        <tr><th>Total files:</th><td>{{runResult.totalFiles}}</td></tr>
      </table>
      <app-job-details [runResultid]='runResult.id'/>
      </div>
  `,
  styleUrl: './run-info-details.css'
})
export class RunInfoDetails {
  @Input() runResult!:RunResults;

  private RefreshTimeInSeconds=10;
  ngOnInit():void {
    if (this.runResult.isActive){
      console.log("starting refresh timer");
      this.startTimer();
    }
  }

  ngOnDestroy() {
  }

  get hostClass(): string {
    if (this.runResult.isActive){
      return "Active";
    }
    return "Inactive";
  }

  startTimer(){

  }

  stopTimer() {
  }

  

}
