import { asyncScheduler } from 'rxjs';
import { delay as higherOrder } from 'rxjs/operators';
export function delay(delay, scheduler) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    return higherOrder(delay, scheduler)(this);
}
//# sourceMappingURL=delay.js.map