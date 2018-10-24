import { Component, NgModule } from '@angular/core';
import { async, ComponentFixture, TestBed, tick, fakeAsync } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef, MatDialogModule } from "@angular/material/dialog";
import { MatButtonModule } from '@angular/material/button';
import { RouterTestingModule } from '@angular/router/testing';
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
    fixture = TestBed.createComponent(LogEntryComponent(mockLogEntry));
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should take input values', fakeAsync(() => {
    
    component.updatePasteSuccess.subscribe((res: any) => { response = res })
    fixture.detectChanges();

    inputTitle = element.querySelector("input");
    inputTitle.value = mockLogEntry.ExecutionId;
    inputTitle.dispatchEvent(new Event("input"));

    expect(mockLogEntry.ExecutionId).toEqual(component.data.ExecutionId);

    component.close();

    //first round of detectChanges()
    fixture.detectChanges();

    //the tick() operation. Don't forget to import tick
    tick();

    //Second round of detectChanges()
    fixture.detectChanges();
    expect(response.title).toEqual(mockLogEntry.title);
    expect(spyOnUpdate.calls.any()).toBe(true, 'updatePaste() method should be called');

  }))
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
