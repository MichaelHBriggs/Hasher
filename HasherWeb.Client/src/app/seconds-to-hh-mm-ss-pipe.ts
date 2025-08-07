import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'secondsToHhMmSsPipe',
  standalone: true
})
export class SecondsToHhMmSsPipe implements PipeTransform {

  transform(totalSeconds: number): string {
    if (isNaN(totalSeconds) || totalSeconds < 0) {
      return '00:00:00'; // Or handle error as needed
    }

    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    const formattedHours = String(hours).padStart(2, '0');
    const formattedMinutes = String(minutes).padStart(2, '0');
    const formattedSeconds = String(seconds.toFixed(2) ).padStart(2, '0');

    return `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
  }

}
