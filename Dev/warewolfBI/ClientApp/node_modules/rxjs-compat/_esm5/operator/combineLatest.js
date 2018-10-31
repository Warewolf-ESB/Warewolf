import { of } from 'rxjs';
import { isArray, CombineLatestOperator } from 'rxjs/internal-compatibility';
export function combineLatest() {
    var observables = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        observables[_i] = arguments[_i];
    }
    var project = null;
    if (typeof observables[observables.length - 1] === 'function') {
        project = observables.pop();
    }
    if (observables.length === 1 && isArray(observables[0])) {
        observables = observables[0].slice();
    }
    return this.lift.call(of.apply(void 0, [this].concat(observables)), new CombineLatestOperator(project));
}
//# sourceMappingURL=combineLatest.js.map