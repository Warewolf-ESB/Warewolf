import { Observable, NEVER } from 'rxjs';
export function staticNever() {
    return NEVER;
}
Observable.never = staticNever;
//# sourceMappingURL=never.js.map