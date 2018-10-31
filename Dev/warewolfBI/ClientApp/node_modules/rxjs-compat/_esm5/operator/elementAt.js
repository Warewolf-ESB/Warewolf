import { elementAt as higherOrder } from 'rxjs/operators';
export function elementAt(index, defaultValue) {
    return higherOrder.apply(undefined, arguments)(this);
}
//# sourceMappingURL=elementAt.js.map