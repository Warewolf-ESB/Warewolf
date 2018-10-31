import { count as higherOrder } from 'rxjs/operators';
export function count(predicate) {
    return higherOrder(predicate)(this);
}
//# sourceMappingURL=count.js.map