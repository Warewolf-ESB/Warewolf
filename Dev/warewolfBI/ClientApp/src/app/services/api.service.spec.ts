import { async, TestBed, inject } from '@angular/core/testing';
import { APIService } from './api.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClientModule, HttpClient } from '@angular/common/http';

describe('Service: APIService', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientModule,
        HttpClientTestingModule
      ]     
    });
  });

  it("default should have url value", async(inject([HttpClient, HttpTestingController], (http: HttpClient) => {
    const expected_WW_API_URL = 'http://localhost:3142';
    const service = new APIService(http);
    expect(service.WW_API_URL).toBe(expected_WW_API_URL);
  })));
  
});
