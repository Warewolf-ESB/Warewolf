import { Observable, defer } from 'rxjs';
export class DeferObservable extends Observable {
    static create(observableFactory) {
        return defer(observableFactory);
    }
}
//# sourceMappingURL=DeferObservable.js.map