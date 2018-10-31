import { throttle as higherOrder } from 'rxjs/operators';
import { defaultThrottleConfig } from 'rxjs/internal-compatibility';
export function throttle(durationSelector, config = defaultThrottleConfig) {
    return higherOrder(durationSelector, config)(this);
}
//# sourceMappingURL=throttle.js.map