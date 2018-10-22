import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { DashboardCard } from '../cards/dashboard-card';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class DashboardService {

  constructor() {
  }

  private _cards: BehaviorSubject<DashboardCard[]> = new BehaviorSubject<DashboardCard[]>([]);

  addCard(card: DashboardCard): void {
    this._cards.next(this._cards.getValue().concat(card));
  }

  get cards(): BehaviorSubject<DashboardCard[]> {
    return this._cards;
  }
}
