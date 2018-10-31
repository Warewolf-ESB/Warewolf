import { publish as higherOrder } from 'rxjs/operators';
export function publish(selector) {
    return higherOrder(selector)(this);
}
//# sourceMappingURL=publish.js.map