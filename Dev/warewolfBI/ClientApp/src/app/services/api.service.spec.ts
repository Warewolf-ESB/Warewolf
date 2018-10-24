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
          provide: Jsonp,
          useFactory: (backend, options) => new Jsonp(backend, options),
          deps: [MockBackend, BaseRequestOptions]
        }
      ]
    });
    backend = TestBed.get(MockBackend);
    service = TestBed.get(APIService);
  });

  it('default should return list of execution logs ', fakeAsync(() => {
    let serverURL = "http://localhost:3142";
    let response = {};
    //let response = {
    //  "resultCount": 1,
    //  "results": [
    //    {
    //      "artistId": 78500,
    //      "artistName": "U2",
    //      "trackName": "Beautiful Day",
    //      "artworkUrl60": "image.jpg",
    //    }]
    //};

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
