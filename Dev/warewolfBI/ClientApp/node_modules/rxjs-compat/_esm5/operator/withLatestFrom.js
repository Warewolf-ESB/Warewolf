import { withLatestFrom as higherOrder } from 'rxjs/operators';
export function withLatestFrom() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
    }
    return higherOrder.apply(void 0, args)(this);
}
//# sourceMappingURL=withLatestFrom.js.map