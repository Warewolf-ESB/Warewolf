import { mergeScan as higherOrder } from 'rxjs/operators';
export function mergeScan(accumulator, seed, concurrent = Number.POSITIVE_INFINITY) {
    return higherOrder(accumulator, seed, concurrent)(this);
}
//# sourceMappingURL=mergeScan.js.map