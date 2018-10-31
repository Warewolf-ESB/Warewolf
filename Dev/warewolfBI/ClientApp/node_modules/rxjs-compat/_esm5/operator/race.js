import { race as higherOrder } from 'rxjs/operators';
export function race() {
    var observables = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        observables[_i] = arguments[_i];
    }
    return higherOrder.apply(void 0, observables)(this);
}
//# sourceMappingURL=race.js.map