import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'secondsToHhMmSsPipe',
  standalone: true
})
export class SecondsToHhMmSsPipe implements PipeTransform {

  transform(totalSeconds: number): string {
    console.log(`SeconsToHhMmSsPipe.Transform(${totalSeconds})`);
    if (isNaN(totalSeconds) || totalSeconds < 0) {
      console.error(`Invalid input: totalSeconds [${totalSeconds}] must be a non-negative number.`);
      return '00:00:00'; // Or handle error as needed
    }

    const days = Math.floor(totalSeconds / 3600 / 24);
    const hours = Math.floor(totalSeconds / 3600) - (days * 24);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;
    

    const formattedDays = String(days).padStart(2, '0');
    const formattedHours = String(hours).padStart(2, '0');
    const formattedMinutes = String(minutes).padStart(2, '0');
    const formattedSeconds = String(seconds.toFixed(2) ).padStart(2, '0');

    var formattedString = '';
    if (days === 0) {
      formattedString = `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
    } else {
      formattedString = `${formattedDays} ${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
    }
    console.log(`For totalSeconds of ${totalSeconds} returning string [${formattedString}]`);
    return formattedString;
  }

}
