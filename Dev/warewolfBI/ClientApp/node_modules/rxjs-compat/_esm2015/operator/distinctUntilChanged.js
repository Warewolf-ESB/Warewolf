import { distinctUntilChanged as higherOrder } from 'rxjs/operators';
export function distinctUntilChanged(compare, keySelector) {
    return higherOrder(compare, keySelector)(this);
}
//# sourceMappingURL=distinctUntilChanged.js.map