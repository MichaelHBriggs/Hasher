import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { JobInfo } from '../DataObjects/jobInfo';
import { JobsRestService } from '../RestServices/jobs.rest.service';

@Component({
  selector: 'app-job-details',
  imports: [ CommonModule, MatTableModule, MatProgressBarModule],
  template: `
     <div class="full-width-container">
  <table mat-table [dataSource]="jobInfos" class="mat-elevation-z8">

    <ng-container matColumnDef="name">
      <th mat-header-cell *matHeaderCellDef>Name</th>
      <td mat-cell *matCellDef="let job">{{job.name}}</td>
    </ng-container>

    <ng-container matColumnDef="rootFolder">
      <th mat-header-cell *matHeaderCellDef>Root Folder</th>
      <td mat-cell *matCellDef="let job">{{job.rootFolder}}</td>
    </ng-container>

    <ng-container matColumnDef="foundFilesCount">
      <th mat-header-cell *matHeaderCellDef># of Found Files</th>
      <td mat-cell *matCellDef="let job">{{job.foundFilesCount}}</td>
    </ng-container>

    <ng-container matColumnDef="processedFilesCount">
      <th mat-header-cell *matHeaderCellDef># of Files Processed </th>
      <td mat-cell *matCellDef="let job">{{job.processedFilesCount}}<br/>
        <mat-progress-bar mode="determinate" value='{{job.percentProcessed}}'></mat-progress-bar>
      </td>
    </ng-container>

    <ng-container matColumnDef="filesHashedCount">
      <th mat-header-cell *matHeaderCellDef># of Files Hashed</th>
      <td mat-cell *matCellDef="let job">{{job.filesHashedCount}}<br/>
        <mat-progress-bar mode="determinate" value='{{job.percentHashed}}'></mat-progress-bar>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

    </table>
  </div>`
,
  styleUrl: './job-details.css'
})
export class JobDetails {
  @Input() runResultid:string="";
  jobInfos:JobInfo[] =[];
  displayedColumns: string[] = ['name', 'rootFolder', 'foundFilesCount', 'filesHashedCount', 'processedFilesCount']; //, '', '', ''
  constructor( private api:JobsRestService){
  }

  ngOnInit():void {
    this.getAllJobInfosForThisRun(this.runResultid);
  }

  getAllJobInfosForThisRun(runId:string){
    console.log("getAllJobInfosForThisRun(${runId})");
    this.api.getJobsForRun(runId).subscribe(data =>{
      this.jobInfos = [];
      console.log("Got " + data.length + " rows back for run ID: " + runId);
      for (const j of data ){
          this.jobInfos.push(j);
      }
    })
  }
}
