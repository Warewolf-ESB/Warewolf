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

describe('LogEntryComponent', () => {
 
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [LogEntryComponent],
      imports: [BrowserModule, RouterTestingModule, FormsModule, ReactiveFormsModule, MatDialogModule, MatButtonModule],
      providers: [
        { provide: MatDialogRef, useValue: {} },
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
});
