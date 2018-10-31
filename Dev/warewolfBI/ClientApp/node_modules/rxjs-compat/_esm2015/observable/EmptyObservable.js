import { Observable, empty } from 'rxjs';
export class EmptyObservable extends Observable {
    static create(scheduler) {
        return empty(scheduler);
    }
}
//# sourceMappingURL=EmptyObservable.js.map