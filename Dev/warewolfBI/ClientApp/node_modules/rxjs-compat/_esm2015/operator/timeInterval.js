import { asyncScheduler } from 'rxjs';
import { timeInterval as higherOrder } from 'rxjs/operators';
export function timeInterval(scheduler = asyncScheduler) {
    return higherOrder(scheduler)(this);
}
//# sourceMappingURL=timeInterval.js.map