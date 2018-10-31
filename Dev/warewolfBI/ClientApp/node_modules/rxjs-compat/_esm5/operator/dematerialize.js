import { dematerialize as higherOrder } from 'rxjs/operators';
export function dematerialize() {
    return higherOrder()(this);
}
//# sourceMappingURL=dematerialize.js.map