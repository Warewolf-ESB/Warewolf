import { publishReplay as higherOrder } from 'rxjs/operators';
export function publishReplay(bufferSize, windowTime, selectorOrScheduler, scheduler) {
    return higherOrder(bufferSize, windowTime, selectorOrScheduler, scheduler)(this);
}
//# sourceMappingURL=publishReplay.js.map