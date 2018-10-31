import { asyncScheduler } from 'rxjs';
import { timeInterval as higherOrder } from 'rxjs/operators';
export function timeInterval(scheduler) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    return higherOrder(scheduler)(this);
}
//# sourceMappingURL=timeInterval.js.map