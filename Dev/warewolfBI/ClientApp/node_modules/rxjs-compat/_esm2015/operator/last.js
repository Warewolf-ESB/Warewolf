import { last as higherOrder } from 'rxjs/operators';
export function last(...args) {
    return higherOrder(...args)(this);
}
//# sourceMappingURL=last.js.map