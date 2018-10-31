import { asyncScheduler } from 'rxjs';
import { isScheduler } from 'rxjs/internal-compatibility';
import { bufferTime as higherOrder } from 'rxjs/operators';
export function bufferTime(bufferTimeSpan) {
    let length = arguments.length;
    let scheduler = asyncScheduler;
    if (isScheduler(arguments[arguments.length - 1])) {
        scheduler = arguments[arguments.length - 1];
        length--;
    }
    let bufferCreationInterval = null;
    if (length >= 2) {
        bufferCreationInterval = arguments[1];
    }
    let maxBufferSize = Number.POSITIVE_INFINITY;
    if (length >= 3) {
        maxBufferSize = arguments[2];
    }
    return higherOrder(bufferTimeSpan, bufferCreationInterval, maxBufferSize, scheduler)(this);
}
//# sourceMappingURL=bufferTime.js.map