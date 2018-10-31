import { toArray as higherOrder } from 'rxjs/operators';
export function toArray() {
    return higherOrder()(this);
}
//# sourceMappingURL=toArray.js.map