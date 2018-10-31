import { distinctUntilKeyChanged as higherOrder } from 'rxjs/operators';
export function distinctUntilKeyChanged(key, compare) {
    return higherOrder(key, compare)(this);
}
//# sourceMappingURL=distinctUntilKeyChanged.js.map