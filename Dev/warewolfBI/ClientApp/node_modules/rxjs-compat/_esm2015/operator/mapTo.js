import { mapTo as higherOrder } from 'rxjs/operators';
export function mapTo(value) {
    return higherOrder(value)(this);
}
//# sourceMappingURL=mapTo.js.map