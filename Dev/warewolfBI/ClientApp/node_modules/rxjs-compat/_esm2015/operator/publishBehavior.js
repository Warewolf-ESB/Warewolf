import { publishBehavior as higherOrder } from 'rxjs/operators';
export function publishBehavior(value) {
    return higherOrder(value)(this);
}
//# sourceMappingURL=publishBehavior.js.map