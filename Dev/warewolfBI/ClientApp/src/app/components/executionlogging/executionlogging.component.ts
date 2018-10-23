import { Component, ElementRef, OnInit, Input, ViewChild, ViewChildren, AfterViewInit, Directive, QueryList, ViewContainerRef, } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
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

@Component({
  selector: 'app-executionlogging',
  templateUrl: './executionlogging.component.html',
  styleUrls: ['./executionlogging.component.scss']
})

export class ExecutionloggingComponent implements OnInit, AfterViewInit {
  logEntry: LogEntry;
  dataSource: ExecutionDataSource;
  displayedColumns = ["ExecutionId", "Url", "ExecutionTime", "Status", "StartDateTime", "CompletedDateTime", "User"];

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChildren('PortInput,serverNameInput,PortInput', { read: ElementRef }) CreateServerURLInput: QueryList<ElementRef>

  serverURL: string;
  serverName: string;
  port: string;
  selected: string;
  protocol: string;
  constructor(private dialog: MatDialog, private route: ActivatedRoute, private executionLoggingservice: ExecutionLoggingService) { }

  ngOnInit() {
    this.selected = '3142';
    this.serverName = "localhost";
    this.logEntry = this.route.snapshot.data["logEntry"];
    this.dataSource = new ExecutionDataSource(this.executionLoggingservice);
    if (this.selected == "3142") { this.protocol = "http"; } else { this.protocol = "https"; }
    this.serverURL = this.protocol + "://" + this.serverName + ":" + this.selected;
    this.dataSource.loadLogs(this.serverURL, '', '', 'asc', 0, 3);
  }

  ngAfterViewInit() {
    this.CreateServerURLInput.changes.subscribe(list => {
      list.forEach(inputs => {
        if (inputs.nativeElement.name == "serverNameInput") { this.serverName = inputs.nativeElement.value; }
        if (inputs.nativeElement.name == "Protocol") { this.protocol = inputs.nativeElement.value; }
        if (this.selected == "3142") { this.protocol = "http"; } else { this.protocol = "https"; }
        this.serverURL = this.protocol + "://" + this.serverName + ":" + this.selected;
      });
    });

    //TODO: This will be added in when we do the paging and sorting in the next ticket
    //fromEvent(this.serverNameInput.nativeElement, 'keyup')
    //  .pipe(
    //    debounceTime(150),
    //    distinctUntilChanged(),
    //    tap(() => {
    //      this.paginator.pageIndex = 0;
    //      this.loadLogsPage();
    //    })
    //  )
    //  .subscribe();

    //this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

    //  merge(this.sort.sortChange, this.paginator.page)
    //    .pipe(
    //      tap(() => this.loadLogsPage())
    //    )
    //    .subscribe();
  }

  loadLogsPage() {
    this.dataSource.loadLogs(
      this.serverURL,
      this.logEntry.ExecutionId,
      this.serverURL,
      this.sort.direction,
      this.paginator.pageIndex,
      this.paginator.pageSize);
  }

  onRowClicked(LogEntry) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = { LogEntry };
    const dialogRef = this.dialog.open(LogEntryComponent, dialogConfig);
  }
}
