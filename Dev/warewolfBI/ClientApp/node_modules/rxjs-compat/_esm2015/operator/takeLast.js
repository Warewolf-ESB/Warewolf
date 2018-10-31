import { takeLast as higherOrderTakeLast } from 'rxjs/operators';
export function takeLast(count) {
    return higherOrderTakeLast(count)(this);
}
//# sourceMappingURL=takeLast.js.map