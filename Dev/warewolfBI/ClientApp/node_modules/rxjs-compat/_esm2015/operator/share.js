import { share as higherOrder } from 'rxjs/operators';
export function share() {
    return higherOrder()(this);
}
//# sourceMappingURL=share.js.map