import { map as higherOrderMap } from 'rxjs/operators';
export function map(project, thisArg) {
    return higherOrderMap(project, thisArg)(this);
}
//# sourceMappingURL=map.js.map