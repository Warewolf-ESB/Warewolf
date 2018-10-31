import { debounce as higherOrder } from 'rxjs/operators';
export function debounce(durationSelector) {
    return higherOrder(durationSelector)(this);
}
//# sourceMappingURL=debounce.js.map