import { expand as higherOrder } from 'rxjs/operators';
export function expand(project, concurrent, scheduler) {
    if (concurrent === void 0) { concurrent = Number.POSITIVE_INFINITY; }
    if (scheduler === void 0) { scheduler = undefined; }
    concurrent = (concurrent || 0) < 1 ? Number.POSITIVE_INFINITY : concurrent;
    return higherOrder(project, concurrent, scheduler)(this);
}
//# sourceMappingURL=expand.js.map