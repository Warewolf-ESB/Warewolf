import { Observable, NEVER } from 'rxjs';
export class NeverObservable extends Observable {
    static create() {
        return NEVER;
    }
}
//# sourceMappingURL=NeverObservable.js.map