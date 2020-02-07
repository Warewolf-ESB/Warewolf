import { NgModule, Component, ElementRef, OnInit, Input, ViewChild, ViewChildren, AfterViewInit, Directive, QueryList, ViewContainerRef, } from '@angular/core';
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
import { MatDialog, MatDialogConfig, MatPaginator, MatSort } from '@angular/material';
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
  imports: [FormsModule, CommonModule],
  declarations: [ExecutionLoggingComponent]
})

export class ExecutionLoggingComponent implements OnInit, AfterViewInit {
  filterForm = new FormGroup({
    filterId: new FormControl(''),
    filterUrl: new FormControl(''),
    filterTime: new FormControl(''),
    filterStatus: new FormControl(''),
    filterStart: new FormControl(''),
    filterComplete: new FormControl(''),
    filterUser: new FormControl(''),
  });
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
  displayedColumns = ['WorkflowName',  'ExecutionId', 'AuditType', 'StartDateTime'];


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
  }
  ChangingPort(event) {
    this.protocol = event;
    if (this.protocol === 'http') { this.port = '3142'; } else { this.port = '3143'; }
    this.serverURL = this.protocol + '://' + this.serverName + ':' + this.port;
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

  onSubmit(buttonType): void {
    if (buttonType === 'Update') {
      this.serverURL = this.protocol + '://' + this.serverName + ':' + this.port;
      const filter = '';
      this.dataSource.loadLogs(this.serverURL, '', filter, 'asc', 0, 3);
    }
    if (buttonType === 'Clear') {
      this.model.filtefilterIdrUrl = '';
      this.model.filterUrl = '';
      this.model.filterTime = '';
      this.model.filterStatus = '';
      this.model.filterStart = '';
      this.model.filterUser = '';
      this.model.filterComplete = '';
    }
  }

  loadLogsPage() {
    this.loading = true;
    this.dataSource.loadLogs(
      this.serverURL,
      this.logEntry.ExecutionID,
      '',
     // this.sort.direction,
    //  this.paginator.pageIndex,
     // this.paginator.pageSize
    );
    this.loading = false;
    this.displayLogs = true;
  }

  // tslint:disable-next-line: no-shadowed-variable
  onRowClicked(LogEntry) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = { LogEntry };
    const dialogRef = this.dialog.open(LogEntryComponent, dialogConfig);
   }
}

