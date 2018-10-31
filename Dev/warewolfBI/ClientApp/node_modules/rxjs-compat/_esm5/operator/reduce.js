import { reduce as higherOrderReduce } from 'rxjs/operators';
export function reduce(accumulator, seed) {
    if (arguments.length >= 2) {
        return higherOrderReduce(accumulator, seed)(this);
    }
    return higherOrderReduce(accumulator)(this);
}
//# sourceMappingURL=reduce.js.map