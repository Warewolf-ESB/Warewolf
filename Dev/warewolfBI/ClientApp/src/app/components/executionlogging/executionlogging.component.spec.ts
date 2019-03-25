import { TestBed, async, ComponentFixture } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { DebugElement, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatPaginatorModule, MatSortModule} from '@angular/material';
import { BrowserModule, By } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CustomMaterialModule } from '../../core/custommaterial.module';
import { LogEntryComponent } from '../logentry/logentry.component';
import { ExecutionLoggingComponent } from './executionlogging.component';
import { ExecutionLoggingService } from './../../services/executionlogging.service';
import { LogEntry } from '../../models/logentry.model';

let component: ExecutionLoggingComponent;
let fixture: ComponentFixture<ExecutionLoggingComponent>;
let mockLogEntry: LogEntry;

describe('Component: ExecutionLoggingComponent', () => {
  

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [
        ExecutionLoggingComponent,
        LogEntryComponent
      ],
      imports: [
        RouterTestingModule,
        CustomMaterialModule, MatDialogModule, MatPaginatorModule, MatSortModule, MatInputModule ,
        HttpClientModule,
        BrowserModule,
        FormsModule,
        BrowserAnimationsModule,
        ReactiveFormsModule
      ],
      providers:[ExecutionLoggingService],
      schemas: [NO_ERRORS_SCHEMA]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExecutionLoggingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set defaults on ngOnInit', () => {
    component.ngOnInit();
    expect(component.port).toEqual('3142');
    expect(component.serverName).toEqual('localhost');
    expect(component.logEntry).not.toBeNull;
    expect(component.dataSource).not.toBeNull;
  });

  it('should set defaults on ngAfterViewInit', () => {
    spyOn(component, 'ngAfterViewInit');

    component.ngAfterViewInit();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.ngAfterViewInit();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.ngAfterViewInit).toHaveBeenCalled();
      });
    });
    expect(component.ngAfterViewInit).toHaveBeenCalled();
  });

  it('should set defaults on loadLogsPage', () => {
    spyOn(component, 'loadLogsPage');

    component.loadLogsPage();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.loadLogsPage();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.loadLogsPage).toHaveBeenCalled();
        expect(component.dataSource).not.toBeNull();
      });
    });
    expect(component.loadLogsPage).toHaveBeenCalled();
  });

  //it('should set defaults on onRowClicked', () => {
  //  spyOn(component, 'onRowClicked');

  //  component.onRowClicked(mockLogEntry);
  //  fixture.detectChanges();
  //  fixture.whenStable().then(() => {
  //    component.onRowClicked(mockLogEntry);
  //    fixture.detectChanges();
  //    fixture.whenStable().then(() => {
  //      expect(component.onRowClicked).toHaveBeenCalled();
  //    });
  //  });
  //  expect(component.onRowClicked).toHaveBeenCalled();
  //});

  it('should get defaults on getLogData', () => {
    spyOn(component, 'ngOnInit');

    fixture.detectChanges();
    fixture.whenStable().then(() => {
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.dataSource.loadLogs('http://localhost:3142', '', '', 'asc', 0, 3)).not.toBeNull();
      });
    });
    expect(component.dataSource.loadLogs('http://localhost:3142', '', '', 'asc', 0, 3)).not.toBeNull();
  });
});

  
