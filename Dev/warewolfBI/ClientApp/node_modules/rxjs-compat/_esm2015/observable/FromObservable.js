import { Observable, from } from 'rxjs';
export class FromObservable extends Observable {
    static create(ish, scheduler) {
        return from(ish, scheduler);
    }
}
//# sourceMappingURL=FromObservable.js.map