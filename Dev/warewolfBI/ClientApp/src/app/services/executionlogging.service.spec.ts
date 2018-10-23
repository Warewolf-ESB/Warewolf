import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { LogEntry } from './../models/logentry.model';
import { ExecutionLoggingService } from './executionlogging.service';
import { APIService } from './api.service';

describe('ExecutionloggingComponent', () => {
  let service: ExecutionLoggingService;
  let serverURL = this.protocol + "http://localhost:3142";
  let apiService: APIService;

  beforeEach(() => {
    service = new ExecutionLoggingService(apiService);
  });

  it('should return a list of execution logs asynchronously', (done: DoneFn) => {
   
    service.getLogData(this.serverURL, '', '', 'asc', 0, 3).subscribe({
      next: (accounts: LogEntry[]) => {
        expect(accounts).toBeTruthy();
        done();
      }
    });
  });
});
