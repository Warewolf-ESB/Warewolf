import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class APIService {
  WW_API_URL = 'http://localhost:3142';
  constructor(protected httpClient: HttpClient) { }

  public handleErrors(error: any): Promise<any> {
    console.error('An error occurred', error);
    return Promise.reject(error.message || error);
  }

  
}
