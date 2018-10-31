import { concatMap as higherOrderConcatMap } from 'rxjs/operators';
export function concatMap(project) {
    return higherOrderConcatMap(project)(this);
}
//# sourceMappingURL=concatMap.js.map