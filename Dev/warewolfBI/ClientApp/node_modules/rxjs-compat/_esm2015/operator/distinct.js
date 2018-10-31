import { distinct as higherOrder } from 'rxjs/operators';
export function distinct(keySelector, flushes) {
    return higherOrder(keySelector, flushes)(this);
}
//# sourceMappingURL=distinct.js.map