import { ExecutionLogging } from '../../models/executionlogging.model';
import { CollectionViewer, DataSource } from "@angular/cdk/collections";
import { MatPaginator, MatSort } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ExecutionLoggingService } from './../../services/executionlogging.service';
import { LogEntry } from './../../models/logentry.model';
import { catchError, finalize } from 'rxjs/operators';
import { of, Observable } from 'rxjs';

export class ExecutionDataSource implements DataSource<LogEntry> {
  logEntry: LogEntry;
  private logsSubject = new BehaviorSubject<LogEntry[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor(private executionLoggingservice: ExecutionLoggingService) { }

  connect(collectionViewer: CollectionViewer): Observable<LogEntry[]> {
    return this.logsSubject.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.logsSubject.complete();
    this.loadingSubject.complete();
  }

  loadLogs(ServerUrl: string, ExecutionID: string, filter = '', sortDirection = 'asc', pageIndex = 0, pageSize = 3) {
    this.loadingSubject.next(true);
    this.executionLoggingservice.getLogData(ServerUrl, ExecutionID, filter, sortDirection, pageIndex, pageSize).pipe(
      catchError(() => of([])),
      finalize(() => this.loadingSubject.next(false))
    ).subscribe(logs => this.logsSubject.next(logs));
  }
}
