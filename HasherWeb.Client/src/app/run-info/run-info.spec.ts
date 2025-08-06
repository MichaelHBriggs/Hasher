import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RunInfo } from './run-info';

describe('RunInfo', () => {
  let component: RunInfo;
  let fixture: ComponentFixture<RunInfo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RunInfo]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RunInfo);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
