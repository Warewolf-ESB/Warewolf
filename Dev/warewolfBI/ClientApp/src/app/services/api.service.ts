import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

import { LogEntry } from './../models/logentry.model';

const httpHeaders = {
  headers: new HttpHeaders({
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Headers': 'X-Requested-With, content-type, Authorization',
    'Content-Type': 'application/json'
  })
};

@Injectable({ providedIn: 'root' })

export class APIService {
  serverUrl: string;
  private headers = new Headers({ 'Content-Type': "application/json" });

  constructor(private httpClient: HttpClient) { }


  login(username: string, password: string) {
    return this.httpClient.post<any>(`${this.serverUrl}/services/Login";`, {})
      .pipe(
        map((response) => { return response; }),
        catchError(this.handleError('login', []))
      );
  }

  getExecutionList(ServerUrl: string, ExecutionId: string, filter = '', sortOrder = 'asc', pageNumber = 0, pageSize = 3): Observable<LogEntry[]> {

    this.serverUrl = ServerUrl.toLowerCase();
    var wareWolfUrl = this.serverUrl + "/services/GetLogDataService";

    //params: new HttpParams()
    //  .set('ExecutionId', ExecutionId.toString())
    //  .set('filter', filter)
    //  .set('sortOrder', sortOrder)
    //  .set('pageNumber', pageNumber.toString())
    //  .set('pageSize', pageSize.toString())

    return this.httpClient.post<any>(wareWolfUrl, filter)
      .pipe(
        map((response) => { return response; }),
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
    console.error(message);
    //TODO: Add to the logging DB;
  }
}
