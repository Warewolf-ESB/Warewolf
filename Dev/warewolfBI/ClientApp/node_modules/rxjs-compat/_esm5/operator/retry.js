import { retry as higherOrder } from 'rxjs/operators';
export function retry(count) {
    if (count === void 0) { count = -1; }
    return higherOrder(count)(this);
}
//# sourceMappingURL=retry.js.map