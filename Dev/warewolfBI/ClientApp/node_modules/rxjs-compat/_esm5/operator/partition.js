import { partition as higherOrder } from 'rxjs/operators';
export function partition(predicate, thisArg) {
    return higherOrder(predicate, thisArg)(this);
}
//# sourceMappingURL=partition.js.map