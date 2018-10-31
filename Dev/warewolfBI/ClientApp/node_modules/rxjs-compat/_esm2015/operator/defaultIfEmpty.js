import { defaultIfEmpty as higherOrder } from 'rxjs/operators';
export function defaultIfEmpty(defaultValue = null) {
    return higherOrder(defaultValue)(this);
}
//# sourceMappingURL=defaultIfEmpty.js.map