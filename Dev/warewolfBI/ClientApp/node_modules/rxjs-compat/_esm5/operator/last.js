import { last as higherOrder } from 'rxjs/operators';
export function last() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
    }
    return higherOrder.apply(void 0, args)(this);
}
//# sourceMappingURL=last.js.map