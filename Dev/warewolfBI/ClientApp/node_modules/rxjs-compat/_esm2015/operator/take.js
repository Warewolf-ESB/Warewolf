import { take as higherOrder } from 'rxjs/operators';
export function take(count) {
    return higherOrder(count)(this);
}
//# sourceMappingURL=take.js.map