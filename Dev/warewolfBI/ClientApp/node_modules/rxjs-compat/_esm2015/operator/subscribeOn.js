import { subscribeOn as higherOrder } from 'rxjs/operators';
export function subscribeOn(scheduler, delay = 0) {
    return higherOrder(scheduler, delay)(this);
}
//# sourceMappingURL=subscribeOn.js.map