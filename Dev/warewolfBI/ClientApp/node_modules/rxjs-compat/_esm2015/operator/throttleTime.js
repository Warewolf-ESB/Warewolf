import { asyncScheduler } from 'rxjs';
import { defaultThrottleConfig } from 'rxjs/internal-compatibility';
import { throttleTime as higherOrder } from 'rxjs/operators';
export function throttleTime(duration, scheduler = asyncScheduler, config = defaultThrottleConfig) {
    return higherOrder(duration, scheduler, config)(this);
}
//# sourceMappingURL=throttleTime.js.map