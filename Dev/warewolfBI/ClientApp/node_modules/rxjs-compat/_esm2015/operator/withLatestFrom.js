import { withLatestFrom as higherOrder } from 'rxjs/operators';
export function withLatestFrom(...args) {
    return higherOrder(...args)(this);
}
//# sourceMappingURL=withLatestFrom.js.map