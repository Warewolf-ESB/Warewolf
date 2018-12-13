import { Component, OnInit, Injectable, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { trigger, state, style, transition, animate, keyframes, query, group } from '@angular/animations';

import { MediaMatcher } from '@angular/cdk/layout';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { FormControl } from '@angular/forms';
import { map } from 'rxjs/operators';
import { MatSidenavContainer } from '@angular/material';
import { NestedTreeControl } from '@angular/cdk/tree';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { Observable,BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

import { MediatorService } from './services/mediator.service';

@Component({
  selector: 'app-root',
  styleUrls: ['./app.component.scss'],
  templateUrl: './app.component.html',
  animations: [
    trigger('toggleSideNav', [
      state('close',
        style({
          width: '50px'
        })
      ),
      state('open',
        style({
          width: '240px'
        })
      ),
      transition('close => open', animate('200ms ease-in')),
      transition('open => close', animate('200ms ease-in')),
    ])
  ]
})

export class AppComponent implements OnInit, OnDestroy {
  title = 'app-root';
  mobileQuery: MediaQueryList;
  opened: boolean;
  closed: boolean;
  events: string[] = [];
  private _mobileQueryListener: () => void;
  panelOpenState = false;
  sideNavState: string = this.mediator.sideNavState;
  overflowState: any = 'auto';
  sideNavText = "Lock Menu";
  sideNavIcon = "lock";

  constructor(
    private mediator: MediatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private media: MediaMatcher,
    private breakpointObserver: BreakpointObserver,
    public router: Router) {
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this._mobileQueryListener = () => changeDetectorRef.detectChanges();
    this.mobileQuery.addListener(this._mobileQueryListener);
  }

  ngOnInit() {
    this.opened = true;
    this.mediator.sideNavListener.subscribe(state => {
      this.sideNavState = state;
    });
  } 

  animationEvent(x) {
    this.overflowState = 'auto';
  }
  
  toggleSideNav() {
    switch (this.sideNavState) {
      case 'close':
        this.sideNavState = 'open';
        this.mediator.setSidenavState(this.sideNavState);
        setTimeout(() => {
          {
            this.sideNavText = this.sideNavText === 'Lock Menu' ? 'Unlock Menu' : 'Lock Menu';
            this.sideNavIcon = this.sideNavIcon === 'lock' ? 'lock_open' : 'lock';
          }
        }, 200);
        break;

      case 'open':
        this.sideNavText = this.sideNavText === 'Lock Menu' ? 'Unlock Menu' : 'Lock Menu';
        this.sideNavIcon = this.sideNavIcon === 'lock' ? 'lock_open' : 'lock';

        setTimeout(() => {
          {
            this.sideNavState = this.sideNavState === 'open' ? 'close' : 'open';
            this.mediator.setSidenavState(this.sideNavState);
          }
        }, 200);
        break;

      default:
        console.log('#6644');
        break;
    }
    this.overflowState = 'hidden';
  }

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches)
    );

  ngOnDestroy(): void {
    this.mobileQuery.removeListener(this._mobileQueryListener);
  }
}
