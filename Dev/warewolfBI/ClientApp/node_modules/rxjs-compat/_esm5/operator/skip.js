import { skip as higherOrder } from 'rxjs/operators';
export function skip(count) {
    return higherOrder(count)(this);
}
//# sourceMappingURL=skip.js.map