import { merge as mergeStatic } from 'rxjs';
export function merge(...observables) {
    return this.lift.call(mergeStatic(this, ...observables));
}
//# sourceMappingURL=merge.js.map