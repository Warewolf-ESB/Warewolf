import { audit as higherOrder } from 'rxjs/operators';
export function audit(durationSelector) {
    return higherOrder(durationSelector)(this);
}
//# sourceMappingURL=audit.js.map