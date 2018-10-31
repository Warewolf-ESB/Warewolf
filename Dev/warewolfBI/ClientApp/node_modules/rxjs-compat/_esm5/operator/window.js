import { window as higherOrder } from 'rxjs/operators';
export function window(windowBoundaries) {
    return higherOrder(windowBoundaries)(this);
}
//# sourceMappingURL=window.js.map