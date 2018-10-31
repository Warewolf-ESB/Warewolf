import { mergeMapTo as higherOrder } from 'rxjs/operators';
export function mergeMapTo(innerObservable, concurrent) {
    if (concurrent === void 0) { concurrent = Number.POSITIVE_INFINITY; }
    return higherOrder(innerObservable, concurrent)(this);
}
//# sourceMappingURL=mergeMapTo.js.map