import { first as higherOrder } from 'rxjs/operators';
export function first(...args) {
    return higherOrder(...args)(this);
}
//# sourceMappingURL=first.js.map