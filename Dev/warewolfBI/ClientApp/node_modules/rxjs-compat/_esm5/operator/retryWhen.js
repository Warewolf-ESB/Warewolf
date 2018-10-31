import { retryWhen as higherOrder } from 'rxjs/operators';
export function retryWhen(notifier) {
    return higherOrder(notifier)(this);
}
//# sourceMappingURL=retryWhen.js.map