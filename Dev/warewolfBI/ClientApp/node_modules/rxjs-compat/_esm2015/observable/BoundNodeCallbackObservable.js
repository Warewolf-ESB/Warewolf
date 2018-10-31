import { Observable, bindNodeCallback } from 'rxjs';
export class BoundNodeCallbackObservable extends Observable {
    static create(func, selector = undefined, scheduler) {
        return bindNodeCallback(func, selector, scheduler);
    }
}
//# sourceMappingURL=BoundNodeCallbackObservable.js.map