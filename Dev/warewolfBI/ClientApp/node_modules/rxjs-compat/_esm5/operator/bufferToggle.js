import { bufferToggle as higherOrder } from 'rxjs/operators';
export function bufferToggle(openings, closingSelector) {
    return higherOrder(openings, closingSelector)(this);
}
//# sourceMappingURL=bufferToggle.js.map