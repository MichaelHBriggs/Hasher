import { SecondsToHhMmSsPipe } from './seconds-to-hh-mm-ss-pipe';

describe('SecondsToHhMmSsPipe', () => {
  it('create an instance', () => {
    const pipe = new SecondsToHhMmSsPipe();
    expect(pipe).toBeTruthy();
  });
});
