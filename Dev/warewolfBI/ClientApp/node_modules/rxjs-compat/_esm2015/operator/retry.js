import { retry as higherOrder } from 'rxjs/operators';
export function retry(count = -1) {
    return higherOrder(count)(this);
}
//# sourceMappingURL=retry.js.map