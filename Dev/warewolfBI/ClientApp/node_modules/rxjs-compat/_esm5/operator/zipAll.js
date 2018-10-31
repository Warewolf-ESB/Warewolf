import { zipAll as higherOrder } from 'rxjs/operators';
export function zipAll(project) {
    return higherOrder(project)(this);
}
//# sourceMappingURL=zipAll.js.map