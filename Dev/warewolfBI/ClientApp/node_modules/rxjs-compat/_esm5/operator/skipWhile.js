import { skipWhile as higherOrder } from 'rxjs/operators';
export function skipWhile(predicate) {
    return higherOrder(predicate)(this);
}
//# sourceMappingURL=skipWhile.js.map