import { concat as concatStatic } from 'rxjs';
export function concat(...observables) {
    return this.lift.call(concatStatic(this, ...observables));
}
//# sourceMappingURL=concat.js.map