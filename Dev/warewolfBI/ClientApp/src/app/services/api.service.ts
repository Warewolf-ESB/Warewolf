import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, catchError } from 'rxjs/operators';
import { LogEntry } from './../models/logentry.model';
import { Observable } from 'rxjs';

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
  constructor(private httpClient: HttpClient) { }

  login(username: string, password: string) {
    return this.httpClient.post<any>(`${this.serverUrl}/services/Login";`, {})
      .map(data => {
        return data;
      });
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
      .pipe(map((response) => {
        return response;
      }), catchError((error) => {
        return error;
      }));
  }
}
