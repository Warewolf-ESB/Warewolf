import { onErrorResumeNext as higherOrder } from 'rxjs/operators';
export function onErrorResumeNext() {
    var nextSources = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        nextSources[_i] = arguments[_i];
    }
    return higherOrder.apply(void 0, nextSources)(this);
}
//# sourceMappingURL=onErrorResumeNext.js.map