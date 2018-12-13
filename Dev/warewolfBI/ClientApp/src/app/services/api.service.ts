import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { LogEntry } from './../models/logentry.model';

const httpOptions = { withCredentials: true };

@Injectable({ providedIn: 'root' })

export class APIService {
  serverUrl: string;
  results: LogEntry[];
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
    let apiURL = `${wareWolfUrl}?ExecutionId=${ExecutionId}&filter=${filter}&sortOrder=${sortOrder}&pageNumber=${pageNumber}&pageSize=${pageSize}&callback=JSONP_CALLBACK`;
    let params = new HttpParams();
    params = params.set('ExecutionId', ExecutionId);
    params = params.set('filter', filter);
    params = params.set('sortOrder', sortOrder);
    params = params.set('pageNumber', pageNumber.toString());
    params = params.set('pageSize', pageSize.toString());
    
    return this.httpClient.get<any[]>(wareWolfUrl, httpOptions)
      .pipe(
        map((response) => {
          this.results = response as LogEntry[];
          return response;
        }),
        catchError(this.handleError('getLogs', []))
      );
  }
  public handleErrors(error: any): Promise<any> {
    console.error('An error occurred', error);
    return Promise.reject(error.message || error);
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
