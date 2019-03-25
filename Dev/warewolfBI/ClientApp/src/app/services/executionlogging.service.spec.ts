import { async, TestBed, inject, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClientModule, HttpClient, HttpRequest, HttpParams } from '@angular/common/http';
import { LogEntry } from './../models/logentry.model';
import { ExecutionLoggingService } from './executionlogging.service';

describe('Service: ExecutionLoggingService', () => {
  let service: ExecutionLoggingService;
  let httpMock: HttpTestingController;
  let http: HttpClient;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientModule,
        HttpClientTestingModule
      ],
      providers: [
        ExecutionLoggingService
      ]
    });
    service = TestBed.get(ExecutionLoggingService);
    httpMock = TestBed.get(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should not immediately connect to the server', () => {
    httpMock.expectNone({});
  });

  describe('when fetching all stuff', () => {
    it('should make a GET request', async(() => {
      var ExecutionId = 'c03bb708-8c1d-4622-9161-8a945e79f6c0';
      var filter = '';
      var sortOrder = 'asc';
      var pageNumber = 0;
      var pageSize = 3;
      var warewolfUrl = "http://localhost:3142/services/GetLogDataService";
      let apiURL = `${warewolfUrl}?ExecutionId='${ExecutionId}'&filter='${filter}'&sortOrder='${sortOrder}'&pageNumber=${pageNumber}&pageSize=${pageSize}`;

      service.getLogData("http://localhost:3142", ExecutionId, filter, sortOrder, pageNumber, pageSize).subscribe();

      let req = httpMock.expectOne(`${apiURL}`);
      expect(req.request.method).toEqual('GET');
      req.flush([]);
    }));

    it('should make a GET request and return results', async(() => {
      var ExecutionId = 'c03bb708-8c1d-4622-9161-8a945e79f6c0';
      var filter = '';
      var sortOrder = 'asc';
      var pageNumber = 0;
      var pageSize = 3;
      var warewolfUrl = "http://localhost:3142/services/GetLogDataService";
      let apiURL = `${warewolfUrl}?ExecutionId='${ExecutionId}'&filter='${filter}'&sortOrder='${sortOrder}'&pageNumber=${pageNumber}&pageSize=${pageSize}`;
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
          "Url": "http://localhost:3142/secure/Test/Api/Unsaved2.xml?<DataList></DataList>&wid=c14edb3d-a116-4b9e-9072-87578dba5ae6",
          "User": "'DEV2\Candice.Daniel'"
        }
      ];

      service.getLogData(`http://localhost:3142`, ExecutionId, filter, sortOrder, pageNumber, pageSize)
        .subscribe(response =>
          expect(response).toEqual(expectedResponse),
        fail
      );

      httpMock.match({
        url: `${apiURL}`,
        method: 'GET'
      })[0].flush(expectedResponse);

    }));
  });
});
