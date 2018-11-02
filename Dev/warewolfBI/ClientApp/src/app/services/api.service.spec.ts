import { TestBed, fakeAsync, tick  } from '@angular/core/testing';
import { JsonpModule, Jsonp, BaseRequestOptions, Response, ResponseOptions, Http } from "@angular/http";
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { MockBackend } from "@angular/http/testing";
import { LogEntry } from './../models/logentry.model';
import { APIService } from './api.service';

describe('Service: APIService', () => {

  let service: APIService;
  let backend: MockBackend; 

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [JsonpModule, HttpClientModule],
      providers: [
        HttpClient,
        APIService,
        MockBackend,
        BaseRequestOptions,
        {
          provide: Http,
          useFactory: (backend, options) => new Http(backend, options),
          deps: [MockBackend, BaseRequestOptions]
        }
      ]
    });
    backend = TestBed.get(MockBackend);
    service = TestBed.get(APIService);
  });

  it('default should return list of execution logs ', fakeAsync(() => {
    let response = {
      "results": [
        {
          "$id": "1",
          "$type": "Dev2.Common.LogEntry, Dev2.Common",
          "StartDateTime": "2018-10-16T12:46:29.834",
          "Status": "ERROR",
          "Url": "http://rsaklfcandice:3142/secure/Ellidex/Api/Unsaved 2.xml?<DataList></DataList>&wid=c14edb3d-a116-4b9e-9072-87578dba5ae6",
          "Result": null,
          "User": "'DEV2\\Candice.Daniel'",
          "CompletedDateTime": "2018-10-16T12:46:29.886",
          "ExecutionTime": "52",
          "ExecutionId": "c03bb708-8c1d-4622-9161-8a945e79f6c0"
        }
      ]
    };

    backend.connections.subscribe(connection => {
      connection.mockRespond(new Response(<ResponseOptions>{
        body: JSON.stringify(response)
      }));
    });
    
    service.getExecutionList("http://localhost:3142", '', '', 'asc', 0, 3);
    tick();
    expect(service).toBeTruthy();
    
    //TODO: mock out results later
    //expect(service.results.length).toBe(1);
    //expect(service.results[0].artist).toBe("U2");
    //expect(service.results[0].name).toBe("Beautiful Day");
    //expect(service.results[0].thumbnail).toBe("image.jpg");
    //expect(service.results[0].artistId).toBe(78500);
  }));
});
