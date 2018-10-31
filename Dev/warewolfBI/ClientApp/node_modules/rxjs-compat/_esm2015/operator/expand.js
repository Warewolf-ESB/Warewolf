import { expand as higherOrder } from 'rxjs/operators';
export function expand(project, concurrent = Number.POSITIVE_INFINITY, scheduler = undefined) {
    concurrent = (concurrent || 0) < 1 ? Number.POSITIVE_INFINITY : concurrent;
    return higherOrder(project, concurrent, scheduler)(this);
}
//# sourceMappingURL=expand.js.map