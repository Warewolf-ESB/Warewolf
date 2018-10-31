import { asyncScheduler } from 'rxjs';
import { timeoutWith as higherOrder } from 'rxjs/operators';
export function timeoutWith(due, withObservable, scheduler) {
    if (scheduler === void 0) { scheduler = asyncScheduler; }
    return higherOrder(due, withObservable, scheduler)(this);
}
//# sourceMappingURL=timeoutWith.js.map