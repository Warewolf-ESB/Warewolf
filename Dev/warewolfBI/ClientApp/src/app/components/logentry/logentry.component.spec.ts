import { Component, NgModule } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';
import { async, ComponentFixture, TestBed, tick, fakeAsync } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef, MatDialogModule } from "@angular/material/dialog";
import { MatButtonModule } from '@angular/material/button';

import { LogEntryComponent } from './logentry.component';
import { LogEntry } from '../../models/logentry.model';

let component: LogEntryComponent;
let fixture: ComponentFixture<LogEntryComponent>;
let mockLogEntry: LogEntry;
let spyOnAdd: jasmine.Spy;

describe('Component: LogEntryComponent', () => {

  // mock object with close method
  const dialogMock = {
    close: () => { }
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [LogEntryComponent],
      imports: [BrowserModule,
        RouterTestingModule,
        FormsModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule],
      providers: [
        { provide: MatDialogRef, useValue: dialogMock },
        { provide: MAT_DIALOG_DATA, useValue: {} },
      ],
      schemas: [NO_ERRORS_SCHEMA]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LogEntryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  it('should resume', () => {
    spyOn(component, 'resume');
    component.resume();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.resume();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.resume).toHaveBeenCalled();
      });
    });
    expect(component.resume).toHaveBeenCalled();
  });
  it('should close', () => {
    spyOn(component, 'close');
    component.close();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.dialogRef.close();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.close).toHaveBeenCalled();
      });
    });
    expect(component.close).toHaveBeenCalled();
  });
});
