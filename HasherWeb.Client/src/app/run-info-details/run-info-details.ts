import { Component, Input, signal, ViewChild, ChangeDetectorRef  } from '@angular/core';
import { MatGridListModule } from '@angular/material/grid-list';
import { RunResults } from "../DataObjects/runResults"
import { CommonModule, formatDate } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { JobDetails } from "../job-details/job-details"
import { SecondsToHhMmSsPipe } from '../seconds-to-hh-mm-ss-pipe';
import { interval, Subscription } from 'rxjs';
import { RunRestService } from '../RestServices/runs.rest.service';

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
export class RunInfoDetails  {
  @Input() runResult!:RunResults;
  @ViewChild(JobDetails) jobDetails!:JobDetails;

  private RefreshTimeInSeconds=1;
  private refreshSubscription: Subscription = Subscription.EMPTY;

  constructor( private api:RunRestService, private cdr: ChangeDetectorRef){}

  ngOnInit():void {
    console.log("ngOnInit()");
    if (this.runResult.isActive){
      console.log("starting refresh timer");
      this.startTimer();
    }
  }

  ngOnDestroy() {
    console.log("ngOnDestroy()");
    this.stopTimer();
  }

  get hostClass(): string {
    if (this.runResult.isActive){
      return "Active";
    }
    return "Inactive";
  }

  startTimer(){
      console.log("startTimer()");
      if (this.refreshSubscription == Subscription.EMPTY){
        this.refreshSubscription = interval(1000 * this.RefreshTimeInSeconds)
          .subscribe(() => this.api.getSpecificRun(this.runResult.id).subscribe(data => {
            console.log("Got fresh run information");
            this.runResult=data;
            if (this.jobDetails){
              this.jobDetails.refreshData();
            }
            this.cdr.markForCheck();
            this.cdr.detectChanges();
            if (!this.runResult.isActive){
              this.stopTimer();
            }
          }));
      }
  }

  stopTimer() {
    console.log("stopTimer()");
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
    }
  }

  

}
