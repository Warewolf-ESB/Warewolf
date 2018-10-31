import { takeUntil as higherOrder } from 'rxjs/operators';
export function takeUntil(notifier) {
    return higherOrder(notifier)(this);
}
//# sourceMappingURL=takeUntil.js.map