import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot } from '@angular/router';
import { AuthenticationService } from './../services/authentication.service';
import decode from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class RoleGuardService implements CanActivate {

  constructor(public auth: AuthenticationService, public router: Router) { }

  canActivate(route: ActivatedRouteSnapshot): boolean {

    const expectedRole = route.data.expectedRole;

    const token = localStorage.getItem('role_type');
    if (token == null) {
      this.router.navigate(['login']);
      return false;
    }

    if (!this.auth.isLoggedIn() || token !== "true") {
      return false;
    }
    return true;
  }

}
