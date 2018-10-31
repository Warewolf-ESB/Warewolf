import { takeWhile as higherOrder } from 'rxjs/operators';
export function takeWhile(predicate) {
    return higherOrder(predicate)(this);
}
//# sourceMappingURL=takeWhile.js.map