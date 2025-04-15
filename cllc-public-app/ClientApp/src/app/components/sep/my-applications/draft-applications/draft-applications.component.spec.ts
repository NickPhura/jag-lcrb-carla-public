import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DraftApplicationsComponent } from './draft-applications.component';

describe('DraftApplicationsComponent', () => {
  let component: DraftApplicationsComponent;
  let fixture: ComponentFixture<DraftApplicationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DraftApplicationsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DraftApplicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
