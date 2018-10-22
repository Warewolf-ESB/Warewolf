import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class MediatorService {
  private _sideState: any = 'open';

  sideNavListener: any = new Subject();

  get sideNavState() {
    return this._sideState;
  }

  setSidenavState(state) {
    this._sideState = state;
  }

  constructor() {
    this.sideNavListener.subscribe(state => {
      this.setSidenavState(state);
    });
  }
}
