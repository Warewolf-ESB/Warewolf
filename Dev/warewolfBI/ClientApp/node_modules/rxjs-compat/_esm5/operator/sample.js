import { sample as higherOrder } from 'rxjs/operators';
export function sample(notifier) {
    return higherOrder(notifier)(this);
}
//# sourceMappingURL=sample.js.map