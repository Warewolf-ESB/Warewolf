import { repeat as higherOrder } from 'rxjs/operators';
export function repeat(count = -1) {
    return higherOrder(count)(this);
}
//# sourceMappingURL=repeat.js.map