import { mergeAll as higherOrder } from 'rxjs/operators';
export function mergeAll(concurrent) {
    if (concurrent === void 0) { concurrent = Number.POSITIVE_INFINITY; }
    return higherOrder(concurrent)(this);
}
//# sourceMappingURL=mergeAll.js.map