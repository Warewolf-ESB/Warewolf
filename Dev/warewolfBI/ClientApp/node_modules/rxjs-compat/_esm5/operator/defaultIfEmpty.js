import { defaultIfEmpty as higherOrder } from 'rxjs/operators';
export function defaultIfEmpty(defaultValue) {
    if (defaultValue === void 0) { defaultValue = null; }
    return higherOrder(defaultValue)(this);
}
//# sourceMappingURL=defaultIfEmpty.js.map