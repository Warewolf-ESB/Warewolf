import { combineAll } from '../../operator/combineAll';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        combineAll: typeof combineAll;
    }
}
