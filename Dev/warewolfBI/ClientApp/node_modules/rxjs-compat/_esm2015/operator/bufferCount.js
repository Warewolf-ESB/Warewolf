import { bufferCount as higherOrder } from 'rxjs/operators';
export function bufferCount(bufferSize, startBufferEvery = null) {
    return higherOrder(bufferSize, startBufferEvery)(this);
}
//# sourceMappingURL=bufferCount.js.map