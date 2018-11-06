import { TestBed, fakeAsync } from '@angular/core/testing';

import { LogEntry } from './../models/logentry.model';
import { APIService } from './api.service';
import { ExecutionLoggingService } from './executionlogging.service';
import { asyncData } from '../../testing';

describe('Service: ExecutionLoggingService', () => {
  let service: ExecutionLoggingService;
  let httpClientSpy: { post: jasmine.Spy };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [
        ExecutionLoggingService
      ]
    });
    httpClientSpy = jasmine.createSpyObj('HttpClient', ['post']);
    service = new ExecutionLoggingService(new APIService(<any>httpClientSpy));
  });

  it('default should return list of execution logs from getLogData', fakeAsync(() => {
    let expectedExecutionLogList: LogEntry[] = [
        {
          "$id": "1",
          "$type": "Dev2.Common.LogEntry, Dev2.Common",
          "CompletedDateTime": "2018-10-16T12:46:29.886",
          "ExecutionId": "c03bb708-8c1d-4622-9161-8a945e79f6c0",
          "ExecutionTime": "52",
          "Result": null,
          "StartDateTime": "2018-10-16T12:46:29.834",
          "Status": "ERROR",
          "Url": "http://rsaklfcandice:3142/secure/Ellidex/Api/Unsaved 2.xml?<DataList></DataList>&wid=c14edb3d-a116-4b9e-9072-87578dba5ae6",
          "User": "'DEV2\Candice.Daniel'"
        }
      ];

    httpClientSpy.post.and.returnValue(asyncData(expectedExecutionLogList));

    service.getLogData("http://localhost:3142", '', '', 'asc', 0, 3)
      .subscribe(
        executionLogList => expect(executionLogList).toEqual(expectedExecutionLogList, 'Expect Execution List'),
        fail
      );
    expect(httpClientSpy.post.calls.count()).toBe(1, 'one call');
  }));
});
