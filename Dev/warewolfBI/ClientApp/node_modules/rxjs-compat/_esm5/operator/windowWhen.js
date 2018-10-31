import { windowWhen as higherOrder } from 'rxjs/operators';
export function windowWhen(closingSelector) {
    return higherOrder(closingSelector)(this);
}
//# sourceMappingURL=windowWhen.js.map