import { Observable, from } from 'rxjs';
export class ArrayLikeObservable extends Observable {
    static create(arrayLike, scheduler) {
        return from(arrayLike, scheduler);
    }
}
//# sourceMappingURL=ArrayLikeObservable.js.map