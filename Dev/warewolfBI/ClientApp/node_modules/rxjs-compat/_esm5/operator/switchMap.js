import { switchMap as higherOrderSwitchMap } from 'rxjs/operators';
export function switchMap(project) {
    return higherOrderSwitchMap(project)(this);
}
//# sourceMappingURL=switchMap.js.map