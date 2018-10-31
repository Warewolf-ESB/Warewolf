import { asyncScheduler } from 'rxjs';
import { defaultThrottleConfig } from 'rxjs/internal-compatibility';
import { throttleTime as higherOrder } from 'rxjs/operators';
export function throttleTime(duration, scheduler, config) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    if (config === void 0) { config = defaultThrottleConfig; }
    return higherOrder(duration, scheduler, config)(this);
}
//# sourceMappingURL=throttleTime.js.map