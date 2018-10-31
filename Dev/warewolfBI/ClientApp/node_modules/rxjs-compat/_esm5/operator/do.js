import { tap as higherOrder } from 'rxjs/operators';
export function _do(nextOrObserver, error, complete) {
    return higherOrder(nextOrObserver, error, complete)(this);
}
//# sourceMappingURL=do.js.map