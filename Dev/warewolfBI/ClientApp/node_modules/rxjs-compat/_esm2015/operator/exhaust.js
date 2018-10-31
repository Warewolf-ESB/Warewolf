import { exhaust as higherOrder } from 'rxjs/operators';
export function exhaust() {
    return higherOrder()(this);
}
//# sourceMappingURL=exhaust.js.map