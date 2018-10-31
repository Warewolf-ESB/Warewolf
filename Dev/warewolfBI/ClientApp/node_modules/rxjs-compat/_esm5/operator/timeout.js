import { asyncScheduler } from 'rxjs';
import { timeout as higherOrder } from 'rxjs/operators';
export function timeout(due, scheduler) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    return higherOrder(due, scheduler)(this);
}
//# sourceMappingURL=timeout.js.map