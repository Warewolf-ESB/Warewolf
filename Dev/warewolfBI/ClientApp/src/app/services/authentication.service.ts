import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';
import * as moment from "moment";
import { User } from '../models/user.model';

import { APIService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {

  redirectUrl: string;
  constructor(private apiService: APIService) { }

  logout() {
    localStorage.removeItem("id_token");
    localStorage.removeItem("role_type");
    localStorage.removeItem("expires_at");
  }

  login(username: string, password: string) {
    return this.apiService.login(username, password);
  }

  setSession(user: User) {
    const expiresAt = moment().add("10000", 'second').toString();
    var roleType = "false";

    if (user.adminAccount) { roleType = "true" }
    localStorage.setItem('id_token', user.userToken);
    localStorage.setItem('role_type', roleType);
    localStorage.setItem("expires_at", expiresAt);
  }

  public isAdmin() {
    return localStorage.getItem('role_type');
  }

  public isLoggedIn() {
    return moment().isBefore(this.getExpiration());
  }

  isLoggedOut() {
    return !this.isLoggedIn();
  }

  getExpiration() {
    const expiration = localStorage.getItem("expires_at");
    const expiresAt = expiration;
    return moment(expiresAt);
  }
}
