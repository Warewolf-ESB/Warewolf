import { materialize as higherOrder } from 'rxjs/operators';
export function materialize() {
    return higherOrder()(this);
}
//# sourceMappingURL=materialize.js.map