import { shareReplay as higherOrder } from 'rxjs/operators';
export function shareReplay(bufferSize, windowTime, scheduler) {
    return higherOrder(bufferSize, windowTime, scheduler)(this);
}
//# sourceMappingURL=shareReplay.js.map