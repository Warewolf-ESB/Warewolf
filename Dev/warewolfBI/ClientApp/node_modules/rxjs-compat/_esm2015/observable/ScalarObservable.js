import { Observable, of } from 'rxjs';
export class ScalarObservable extends Observable {
    static create(value, scheduler) {
        return arguments.length > 1 ? of(value, scheduler) : of(value);
    }
}
//# sourceMappingURL=ScalarObservable.js.map