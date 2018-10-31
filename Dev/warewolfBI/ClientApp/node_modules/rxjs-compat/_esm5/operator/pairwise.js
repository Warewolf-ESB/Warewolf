import { pairwise as higherOrder } from 'rxjs/operators';
export function pairwise() {
    return higherOrder()(this);
}
//# sourceMappingURL=pairwise.js.map