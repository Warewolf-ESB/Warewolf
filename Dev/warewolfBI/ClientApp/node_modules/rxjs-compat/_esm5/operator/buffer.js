import { buffer as higherOrder } from 'rxjs/operators';
export function buffer(closingNotifier) {
    return higherOrder(closingNotifier)(this);
}
//# sourceMappingURL=buffer.js.map