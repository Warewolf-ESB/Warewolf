import { startWith as higherOrder } from 'rxjs/operators';
export function startWith(...array) {
    return higherOrder(...array)(this);
}
//# sourceMappingURL=startWith.js.map