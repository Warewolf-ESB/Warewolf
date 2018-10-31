import { single as higherOrder } from 'rxjs/operators';
export function single(predicate) {
    return higherOrder(predicate)(this);
}
//# sourceMappingURL=single.js.map