import { filter as higherOrderFilter } from 'rxjs/operators';
export function filter(predicate, thisArg) {
    return higherOrderFilter(predicate, thisArg)(this);
}
//# sourceMappingURL=filter.js.map