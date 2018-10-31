import { Observable, from } from 'rxjs';
export class IteratorObservable extends Observable {
    static create(iterator, scheduler) {
        return from(iterator, scheduler);
    }
}
//# sourceMappingURL=IteratorObservable.js.map