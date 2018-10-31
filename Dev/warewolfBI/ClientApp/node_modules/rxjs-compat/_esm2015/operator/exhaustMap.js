import { exhaustMap as higherOrder } from 'rxjs/operators';
export function exhaustMap(project) {
    return higherOrder(project)(this);
}
//# sourceMappingURL=exhaustMap.js.map