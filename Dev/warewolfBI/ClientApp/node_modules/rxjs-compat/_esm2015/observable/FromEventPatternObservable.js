import { Observable, fromEventPattern } from 'rxjs';
export class FromEventPatternObservable extends Observable {
    static create(addHandler, removeHandler, selector) {
        return fromEventPattern(addHandler, removeHandler, selector);
    }
}
//# sourceMappingURL=FromEventPatternObservable.js.map