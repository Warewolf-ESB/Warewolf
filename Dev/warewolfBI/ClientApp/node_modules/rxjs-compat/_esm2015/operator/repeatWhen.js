import { repeatWhen as higherOrder } from 'rxjs/operators';
export function repeatWhen(notifier) {
    return higherOrder(notifier)(this);
}
//# sourceMappingURL=repeatWhen.js.map