import { asyncScheduler } from 'rxjs';
import { auditTime as higherOrder } from 'rxjs/operators';
export function auditTime(duration, scheduler = asyncScheduler) {
    return higherOrder(duration, scheduler)(this);
}
//# sourceMappingURL=auditTime.js.map