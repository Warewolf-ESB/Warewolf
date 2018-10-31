import { mergeAll as higherOrder } from 'rxjs/operators';
export function mergeAll(concurrent = Number.POSITIVE_INFINITY) {
    return higherOrder(concurrent)(this);
}
//# sourceMappingURL=mergeAll.js.map