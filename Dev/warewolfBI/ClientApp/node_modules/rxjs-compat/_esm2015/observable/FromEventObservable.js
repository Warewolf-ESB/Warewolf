import { Observable, fromEvent } from 'rxjs';
export class FromEventObservable extends Observable {
    static create(target, eventName, options, selector) {
        return fromEvent(target, eventName, options, selector);
    }
}
//# sourceMappingURL=FromEventObservable.js.map