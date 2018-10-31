import { observeOn as higherOrder } from 'rxjs/operators';
export function observeOn(scheduler, delay) {
    if (delay === void 0) { delay = 0; }
    return higherOrder(scheduler, delay)(this);
}
//# sourceMappingURL=observeOn.js.map