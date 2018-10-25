import { TestBed, async, ComponentFixture } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { DebugElement, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatPaginatorModule, MatSortModule} from '@angular/material';
import { BrowserModule, By } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CustomMaterialModule } from '../../core/custommaterial.module';
import { LogEntryComponent } from '../logentry/logentry.component';
import { ExecutionLoggingComponent } from './executionlogging.component';
import { ExecutionLoggingService } from './../../services/executionlogging.service';

describe('ExecutionLoggingComponent', () => {
  let component: ExecutionLoggingComponent;
  let fixture: ComponentFixture<ExecutionLoggingComponent>;

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
        BrowserAnimationsModule
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
});

  
