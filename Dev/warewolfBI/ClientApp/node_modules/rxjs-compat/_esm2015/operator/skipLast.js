import { skipLast as higherOrder } from 'rxjs/operators';
export function skipLast(count) {
    return higherOrder(count)(this);
}
//# sourceMappingURL=skipLast.js.map