import { Observable, asyncScheduler, interval } from 'rxjs';
export class IntervalObservable extends Observable {
    static create(period = 0, scheduler = asyncScheduler) {
        return interval(period, scheduler);
    }
}
//# sourceMappingURL=IntervalObservable.js.map