import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RunInfoDetails } from './run-info-details';

describe('RunInfoDetails', () => {
  let component: RunInfoDetails;
  let fixture: ComponentFixture<RunInfoDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RunInfoDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RunInfoDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
