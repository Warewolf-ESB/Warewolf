import { combineAll as higherOrder } from 'rxjs/operators';
export function combineAll(project) {
    return higherOrder(project)(this);
}
//# sourceMappingURL=combineAll.js.map