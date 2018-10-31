import { asyncScheduler } from 'rxjs';
import { debounceTime as higherOrder } from 'rxjs/operators';
export function debounceTime(dueTime, scheduler = asyncScheduler) {
    return higherOrder(dueTime, scheduler)(this);
}
//# sourceMappingURL=debounceTime.js.map