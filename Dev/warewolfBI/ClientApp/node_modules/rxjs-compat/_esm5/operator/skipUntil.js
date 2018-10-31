import { skipUntil as higherOrder } from 'rxjs/operators';
export function skipUntil(notifier) {
    return higherOrder(notifier)(this);
}
//# sourceMappingURL=skipUntil.js.map