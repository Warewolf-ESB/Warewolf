import { toArray } from '../../operator/toArray';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        toArray: typeof toArray;
    }
}
