import { findIndex as higherOrder } from 'rxjs/operators';
export function findIndex(predicate, thisArg) {
    return higherOrder(predicate, thisArg)(this);
}
//# sourceMappingURL=findIndex.js.map