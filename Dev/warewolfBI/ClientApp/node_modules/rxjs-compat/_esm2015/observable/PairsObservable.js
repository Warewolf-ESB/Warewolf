import { Observable, pairs } from 'rxjs';
export class PairsObservable extends Observable {
    static create(obj, scheduler) {
        return pairs(obj, scheduler);
    }
}
//# sourceMappingURL=PairsObservable.js.map