import { asyncScheduler } from 'rxjs';
import { sampleTime as higherOrder } from 'rxjs/operators';
export function sampleTime(period, scheduler) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    return higherOrder(period, scheduler)(this);
}
//# sourceMappingURL=sampleTime.js.map