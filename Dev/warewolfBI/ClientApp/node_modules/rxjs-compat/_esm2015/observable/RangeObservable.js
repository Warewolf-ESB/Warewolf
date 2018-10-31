import { Observable, range } from 'rxjs';
export class RangeObservable extends Observable {
    static create(start = 0, count = 0, scheduler) {
        return range(start, count, scheduler);
    }
}
//# sourceMappingURL=RangeObservable.js.map