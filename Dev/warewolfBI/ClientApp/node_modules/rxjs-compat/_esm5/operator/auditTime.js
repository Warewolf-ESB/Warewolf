import { asyncScheduler } from 'rxjs';
import { auditTime as higherOrder } from 'rxjs/operators';
export function auditTime(duration, scheduler) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    return higherOrder(duration, scheduler)(this);
}
//# sourceMappingURL=auditTime.js.map