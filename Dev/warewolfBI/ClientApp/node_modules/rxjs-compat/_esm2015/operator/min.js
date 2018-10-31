import { min as higherOrderMin } from 'rxjs/operators';
export function min(comparer) {
    return higherOrderMin(comparer)(this);
}
//# sourceMappingURL=min.js.map