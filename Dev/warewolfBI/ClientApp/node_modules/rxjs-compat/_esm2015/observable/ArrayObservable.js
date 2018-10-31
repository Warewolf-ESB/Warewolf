import { Observable, from } from 'rxjs';
export class ArrayObservable extends Observable {
    static create(array, scheduler) {
        return from(array, scheduler);
    }
}
//# sourceMappingURL=ArrayObservable.js.map