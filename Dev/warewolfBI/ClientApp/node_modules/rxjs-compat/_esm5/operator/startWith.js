import { startWith as higherOrder } from 'rxjs/operators';
export function startWith() {
    var array = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        array[_i] = arguments[_i];
    }
    return higherOrder.apply(void 0, array)(this);
}
//# sourceMappingURL=startWith.js.map