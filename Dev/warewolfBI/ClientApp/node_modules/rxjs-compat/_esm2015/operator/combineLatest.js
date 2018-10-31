import { of } from 'rxjs';
import { isArray, CombineLatestOperator } from 'rxjs/internal-compatibility';
export function combineLatest(...observables) {
    let project = null;
    if (typeof observables[observables.length - 1] === 'function') {
        project = observables.pop();
    }
    if (observables.length === 1 && isArray(observables[0])) {
        observables = observables[0].slice();
    }
    return this.lift.call(of(this, ...observables), new CombineLatestOperator(project));
}
//# sourceMappingURL=combineLatest.js.map