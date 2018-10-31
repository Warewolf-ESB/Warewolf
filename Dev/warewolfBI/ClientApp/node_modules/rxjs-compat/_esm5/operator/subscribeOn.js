import { subscribeOn as higherOrder } from 'rxjs/operators';
export function subscribeOn(scheduler, delay) {
    if (delay === void 0) { delay = 0; }
    return higherOrder(scheduler, delay)(this);
}
//# sourceMappingURL=subscribeOn.js.map