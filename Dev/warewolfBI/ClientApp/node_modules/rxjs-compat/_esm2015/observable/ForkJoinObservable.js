import { Observable, forkJoin } from 'rxjs';
export class ForkJoinObservable extends Observable {
    static create(...sources) {
        return forkJoin(...sources);
    }
}
//# sourceMappingURL=ForkJoinObservable.js.map