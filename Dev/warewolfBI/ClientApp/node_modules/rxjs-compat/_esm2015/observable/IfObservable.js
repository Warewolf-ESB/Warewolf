import { Observable, iif } from 'rxjs';
export class IfObservable extends Observable {
    static create(condition, thenSource, elseSource) {
        return iif(condition, thenSource, elseSource);
    }
}
//# sourceMappingURL=IfObservable.js.map