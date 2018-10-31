import { every as higherOrder } from 'rxjs/operators';
export function every(predicate, thisArg) {
    return higherOrder(predicate, thisArg)(this);
}
//# sourceMappingURL=every.js.map