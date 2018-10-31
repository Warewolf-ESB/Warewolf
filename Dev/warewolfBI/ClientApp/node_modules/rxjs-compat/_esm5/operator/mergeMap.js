import { mergeMap as higherOrderMergeMap } from 'rxjs/operators';
export function mergeMap(project, concurrent) {
    if (concurrent === void 0) { concurrent = Number.POSITIVE_INFINITY; }
    return higherOrderMergeMap(project, concurrent)(this);
}
//# sourceMappingURL=mergeMap.js.map