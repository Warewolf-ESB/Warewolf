import { asyncScheduler } from 'rxjs';
import { timestamp as higherOrder } from 'rxjs/operators';
export function timestamp(scheduler = asyncScheduler) {
    return higherOrder(scheduler)(this);
}
//# sourceMappingURL=timestamp.js.map