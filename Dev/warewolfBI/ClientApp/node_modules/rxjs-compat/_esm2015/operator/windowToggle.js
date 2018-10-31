import { windowToggle as higherOrder } from 'rxjs/operators';
export function windowToggle(openings, closingSelector) {
    return higherOrder(openings, closingSelector)(this);
}
//# sourceMappingURL=windowToggle.js.map