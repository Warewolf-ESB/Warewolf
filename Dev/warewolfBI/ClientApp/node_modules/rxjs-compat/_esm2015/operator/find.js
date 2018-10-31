import { find as higherOrder } from 'rxjs/operators';
export function find(predicate, thisArg) {
    return higherOrder(predicate, thisArg)(this);
}
//# sourceMappingURL=find.js.map