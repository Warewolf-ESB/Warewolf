import { throttle as higherOrder } from 'rxjs/operators';
import { defaultThrottleConfig } from 'rxjs/internal-compatibility';
export function throttle(durationSelector, config) {
    if (config === void 0) { config = defaultThrottleConfig; }
    return higherOrder(durationSelector, config)(this);
}
//# sourceMappingURL=throttle.js.map