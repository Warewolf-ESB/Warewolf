import { pluck as higherOrder } from 'rxjs/operators';
export function pluck(...properties) {
    return higherOrder(...properties)(this);
}
//# sourceMappingURL=pluck.js.map