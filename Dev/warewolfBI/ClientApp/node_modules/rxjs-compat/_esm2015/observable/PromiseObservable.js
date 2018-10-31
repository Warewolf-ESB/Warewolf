import { Observable, from } from 'rxjs';
export class PromiseObservable extends Observable {
    static create(promise, scheduler) {
        return from(promise, scheduler);
    }
}
//# sourceMappingURL=PromiseObservable.js.map