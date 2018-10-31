import { pluck as higherOrder } from 'rxjs/operators';
export function pluck() {
    var properties = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        properties[_i] = arguments[_i];
    }
    return higherOrder.apply(void 0, properties)(this);
}
//# sourceMappingURL=pluck.js.map