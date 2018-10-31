import { max as higherOrderMax } from 'rxjs/operators';
export function max(comparer) {
    return higherOrderMax(comparer)(this);
}
//# sourceMappingURL=max.js.map