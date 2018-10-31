import { Observable, timer } from 'rxjs';
export class TimerObservable extends Observable {
    static create(initialDelay = 0, period, scheduler) {
        return timer(initialDelay, period, scheduler);
    }
}
//# sourceMappingURL=TimerObservable.js.map