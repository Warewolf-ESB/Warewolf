import { Injectable } from '@angular/core';
import 'rxjs/add/operator/map';
import { map, catchError } from 'rxjs/operators';
import { APIService } from './api.service';
import { LogEntry } from './../models/logentry.model';
import { HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';

const httpOptions = { withCredentials: true };
@Injectable({
  providedIn: 'root',
})

export class ExecutionLoggingService extends APIService {
  serverUrl: string;
  results: LogEntry[];

  getLogData(ServerUrl: string, ExecutionId: string, filter = '', sortOrder = 'asc', pageNumber = 0, pageSize = 3): Observable<LogEntry[]> {

    this.serverUrl = ServerUrl.toLowerCase();
    var warewolfUrl = this.serverUrl + "/services/GetLogDataService";
    let apiURL = `${warewolfUrl}?ExecutionId='${ExecutionId}'&filter='${filter}'&sortOrder='${sortOrder}'&pageNumber=${pageNumber}&pageSize=${pageSize}`;
    let params = new HttpParams();
    params = params.set('ExecutionId', ExecutionId);
    params = params.set('filter', filter);
    params = params.set('sortOrder', sortOrder);
    params = params.set('pageNumber', pageNumber.toString());
    params = params.set('pageSize', pageSize.toString());

    return this.httpClient.get<any[]>(apiURL, httpOptions)
      .pipe(
        map((response) => {
          this.results = response as LogEntry[];
          return response;
        }),
        catchError(this.handleError('getLogs', []))
      );
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      this.log(`${operation} failed: ${error.message}`);
      return of(result as T);
    };
  }

  private log(message: string) {
    //TODO: Add to the logging DB
  }
}
