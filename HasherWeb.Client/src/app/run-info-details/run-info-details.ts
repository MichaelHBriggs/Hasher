import { Component, Input } from '@angular/core';
import { RunResults } from "../DataObjects/runResults"

@Component({
  selector: 'app-run-info-details',
  imports: [],
  template: `
    <section class='runDetails'>
      <table>
        <tr><th>Started at:</th><td>{{runResult.createdAt}}</td></tr>
        <tr><th>Total files:</th><td>{{runResult.totalFiles}}</td></tr>
      </table>
    </section>
  `,
  styleUrl: './run-info-details.css'
})
export class RunInfoDetails {
  @Input() runResult!:RunResults;
}
