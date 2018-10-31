import { publishLast as higherOrder } from 'rxjs/operators';
export function publishLast() {
    return higherOrder()(this);
}
//# sourceMappingURL=publishLast.js.map