import { Component, signal } from '@angular/core';
import { RunInfo } from '../app/run-info/run-info'
import { LogInfo } from '../app/log-info/log-info'

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Hasher Web Client');
}
