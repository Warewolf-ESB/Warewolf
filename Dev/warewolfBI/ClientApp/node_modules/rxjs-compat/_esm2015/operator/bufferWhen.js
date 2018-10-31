import { bufferWhen as higherOrder } from 'rxjs/operators';
export function bufferWhen(closingSelector) {
    return higherOrder(closingSelector)(this);
}
//# sourceMappingURL=bufferWhen.js.map