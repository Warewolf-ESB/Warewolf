import { concatMapTo as higherOrder } from 'rxjs/operators';
export function concatMapTo(innerObservable) {
    return higherOrder(innerObservable)(this);
}
//# sourceMappingURL=concatMapTo.js.map