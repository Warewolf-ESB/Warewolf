import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { APIService } from './api.service';
import { LogEntry } from './../models/logentry.model';

@Injectable()
export class ExecutionLoggingService {
  constructor(private apiService: APIService) { }

  getLogData(ServerUrl:string,ExecutionId: string, filter = '', sortOrder = 'asc', pageNumber = 0, pageSize = 3): Observable<LogEntry[]> {
    return this.apiService.getExecutionList(ServerUrl,ExecutionId, filter, sortOrder,pageNumber, pageSize);
  }
}
