import { NgModule, Component, Pipe, PipeTransform, ElementRef, OnInit, Input, ViewChild, ViewChildren, AfterViewInit, Directive, QueryList, ViewContainerRef, } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, FormBuilder, Validators } from '@angular/forms';
import 'rxjs/add/observable/fromEvent';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import { ExecutionLoggingService } from './../../services/executionlogging.service';
import { ActivatedRoute } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogConfig, MatDatepickerModule, MatNativeDateModule, MatProgressSpinnerModule, MatPaginator, MatSort } from '@angular/material';
import { MatButtonModule } from '@angular/material/button';
import { ExecutionDataSource } from './executionLoggingDataSource';
import { ExecutionLogging } from './../../models/executionlogging.model';
import { LogEntry } from './../../models/logentry.model';
import { tap, distinctUntilChanged, debounceTime } from 'rxjs/operators';
import { fromEvent, merge } from 'rxjs';
import { LogEntryComponent } from '../logentry/logentry.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-executionlogging',
  templateUrl: './executionlogging.component.html',
  styleUrls: ['./executionlogging.component.scss']
})

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    MatNativeDateModule,
    MatInputModule,
    MatDatepickerModule,
    MatProgressSpinnerModule],
  declarations: [ExecutionLoggingComponent]

})

export class ExecutionLoggingComponent implements OnInit, AfterViewInit {
  filterForm = new FormGroup({
    ServerID: new FormControl(''),
    WorkflowID: new FormControl(''),
    WorkflowName: new FormControl(''),
    ExecutionID: new FormControl(''),
    ParentID: new FormControl(''),
    PreviousActivityId: new FormControl(''),
    IsSubExecution: new FormControl(''),
    IsRemoteWorkflow: new FormControl(''),
    StartDateTime: new FormControl(new Date()),
    CompletedDateTime: new FormControl(new Date())
  });
  oneday = 1000 * 60 * 60 * 24;
  error: any = { isError: false, errorMessage: '' };
  isValidDate: any;
  model: any = {};
  displayLogs = false;
  logEntry: LogEntry;
  dataSource: ExecutionDataSource;
  serverURL: string;
  serverName = 'localhost';
  port = '3142';
  protocol = 'http';
  loading = true;
  portSelect = 'http';
  displayedColumns = ['ServerID', 'WorkflowID', 'WorkflowName', 'ExecutionID', 'ParentID', 'PreviousActivityId', 'IsSubExecution', 'IsRemoteWorkflow', 'AuditType', 'StartDateTime'];

  constructor(private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private route: ActivatedRoute,
    private executionLoggingservice: ExecutionLoggingService) { }

  ngOnInit() {
    this.displayLogs = false;
    this.loading = true;
    this.protocol = 'http';
    this.port = '3142';
    this.serverName = 'localhost';
    this.logEntry = this.route.snapshot.data['logEntry'];
    this.dataSource = new ExecutionDataSource(this.executionLoggingservice);
    this.serverURL = this.protocol + '://' + this.serverName + ':' + this.port;
    this.dataSource.loadLogs(this.serverURL, '', '', 'asc', 0, 3);
    this.loading = false;
    this.displayLogs = true;
    this.model.ServerID = '';
    this.model.WorkflowID = '';
    this.model.WorkflowName = '';
    this.model.ExecutionID = '';
    this.model.ParentID = '';
    this.model.PreviousActivityId = '';
    this.model.IsSubExecution = '';
    this.model.IsRemoteWorkflow = '';
    this.model.StartDateTime = new Date((new Date().getTime() - this.oneday));
    this.model.CompletedDateTime = new Date((new Date().getTime()));
  }
  ChangingPort(event) {
    this.protocol = event;
    if (this.protocol === 'http') { this.port = '3142'; } else { this.port = '3143'; }
    this.serverURL = this.protocol + '://' + this.serverName + ':' + this.port;
  }

  onSubmit(buttonType): void {
    if (buttonType === 'Update') {
      this.serverURL = this.protocol.replace(/\s/g, '') + '://' + this.serverName.replace(/\s/g, '') + ':' + this.port.replace(/\s/g, '');
      const filter = '';
      // this.dataSource.loadLogs(this.serverURL,  this.model.ExecutionID.replace(/\s/g, ''), filter, 'asc', 0, 3);
    }
    if (buttonType === 'Clear') {
      this.model.ServerID = '';
      this.model.WorkflowID = '';
      this.model.WorkflowName = '';
      this.model.ExecutionID = '';
      this.model.ParentID = '';
      this.model.PreviousActivityId = '';
      this.model.IsSubExecution = '';
      this.model.IsRemoteWorkflow = '';
      this.model.StartDateTime = new Date((new Date().getTime() - this.oneday));
      this.model.CompletedDateTime = new Date((new Date().getTime()));
    }
    if (buttonType === 'Filter') {
      console.log(this.model);
      this.isValidDate = this.validateDates(this.model.StartDateTime, this.model.CompletedDateTime);
      if (this.isValidDate) {
        this.serverURL = this.protocol.replace(/\s/g, '') + '://' + this.serverName.replace(/\s/g, '') + ':' + this.port.replace(/\s/g, '');
        this.dataSource.loadLogs(this.serverURL, this.model.ExecutionID, JSON.stringify(this.model), 'asc', 0, 3);
      }
    }
  }

  loadLogsPage() {
    this.loading = true;
    this.dataSource.loadLogs(
      this.serverURL,
      this.model.ExecutionID,
      JSON.stringify(this.model),
      // this.sort.direction,
      //  this.paginator.pageIndex,
      // this.paginator.pageSize
    );
    this.loading = false;
    this.displayLogs = true;
  }

  onRowClicked(LogEntry: any) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = { LogEntry };
    const dialogRef = this.dialog.open(LogEntryComponent, dialogConfig);
  }

  ngAfterViewInit() {
    // TODO: This will be added in when we do the paging and sorting in the next ticket
    // fromEvent(this.serverNameInput.nativeElement, 'keyup')
    //  .pipe(
    //    debounceTime(150),
    //    distinctUntilChanged(),
    //    tap(() => {
    //      this.paginator.pageIndex = 0;
    //      this.loadLogsPage();
    //    })
    //  )
    //  .subscribe();

    // this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

    //  merge(this.sort.sortChange, this.paginator.page)
    //    .pipe(
    //      tap(() => this.loadLogsPage())
    //    )
    //    .subscribe();
  }

  changeStartDate(event) {
    this.model.StartDateTime = event.value;

  }
  changeEndDate(event) {
    this.model.CompletedDateTime = event.value;

  }
  validateDates(sDate: string, eDate: string) {
    this.isValidDate = true;
    if ((sDate == null || eDate == null)) {
      this.error = { isError: true, errorMessage: 'Start date and end date are required.' };
      this.isValidDate = false;
    }

    if ((sDate != null && eDate != null) && (eDate) < (sDate)) {
      this.error = { isError: true, errorMessage: 'End date should be grater then start date.' };
      this.isValidDate = false;
    }
    return this.isValidDate;
  }
}

