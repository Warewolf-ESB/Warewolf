import { async, TestBed, inject } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClientModule, HttpClient, HttpRequest } from '@angular/common/http';
import { LogEntry } from './../models/logentry.model';
import { ExecutionLoggingService } from './executionlogging.service';
import { asyncData } from '../../testing';

describe('Service: ExecutionLoggingService', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientModule,
        HttpClientTestingModule
      ]
    });
  });

  it('default should return list of execution logs from getLogData', async(inject([HttpClient, HttpTestingController], (http: HttpClient, backend: HttpTestingController) => {

    var service = new ExecutionLoggingService(http)

    var ExecutionId = '';
    var filter = '';
    var sortOrder = 'asc';
    var pageNumber = 0;
    var pageSize = 3;
    let apiURL = `http://localhost:3142/services/GetLogDataService?ExecutionId=${ExecutionId}&filter=${filter}&sortOrder=${sortOrder}&pageNumber=${pageNumber}&pageSize=${pageSize}&callback=JSONP_CALLBACK`;

    let expectedResponse: LogEntry[] = [
      {
        "$id": "1",
        "$type": "Dev2.Common.LogEntry, Dev2.Common",
        "CompletedDateTime": "2018-10-16T12:46:29.886",
        "ExecutionId": "c03bb708-8c1d-4622-9161-8a945e79f6c0",
        "ExecutionTime": "52",
        "Result": null,
        "StartDateTime": "2018-10-16T12:46:29.834",
        "Status": "ERROR",
        "Url": "http://localhost:3142/secure/Test/Api/Unsaved 2.xml?<DataList></DataList>&wid=c14edb3d-a116-4b9e-9072-87578dba5ae6",
        "User": "'DEV2\Candice.Daniel'"
      }
    ];

    service.getLogData(`http://localhost:3142`, ExecutionId, filter, sortOrder, pageNumber, pageSize).subscribe(
      response => expect(response).toEqual(expectedResponse, 'Expect Execution List'),
        fail
      );

    var calls = backend.match((req: HttpRequest<any>) => {
      var request = req.url === `http://localhost:3142/services/GetLogDataService` &&
        req.urlWithParams == `${apiURL}` &&
        req.body == null &&
        req.method === 'GET';
      return request;
    });

    if (calls.length > 0) {
      calls[0].flush(expectedResponse);
    }
    backend.expectOne(`http://localhost:3142/services/GetLogDataService`).flush(expectedResponse);
    backend.verify();
  })));
});
