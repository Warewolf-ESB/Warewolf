import { switchMapTo as higherOrder } from 'rxjs/operators';
export function switchMapTo(innerObservable) {
    return higherOrder(innerObservable)(this);
}
//# sourceMappingURL=switchMapTo.js.map