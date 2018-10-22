import { Injectable } from '@angular/core';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import { User } from './../models/user.model';

import { APIService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(private apiService: APIService) { }

}
