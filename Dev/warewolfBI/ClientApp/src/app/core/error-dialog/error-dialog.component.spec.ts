import { Component, NgModule } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef, MatDialogModule } from "@angular/material/dialog";
import { ErrorDialogComponent } from './error-dialog.component';
import { MatButtonModule } from '@angular/material/button';

let component: ErrorDialogComponent;
let fixture: ComponentFixture<ErrorDialogComponent>;

describe('ErrorDialogComponent', () => {

  // mock object with close method
  const dialogMock = {
    close: () => { }
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ErrorDialogComponent, MatDialogRef],
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
    fixture = TestBed.createComponent(ErrorDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  it('should close', () => {
    spyOn(component, 'close');
    component.close();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.close();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.close).toHaveBeenCalled();
      });
    });
    expect(component.close).toHaveBeenCalled();
  });
});
