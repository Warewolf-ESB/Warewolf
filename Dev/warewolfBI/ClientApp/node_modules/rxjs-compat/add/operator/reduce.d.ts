import { reduce } from '../../operator/reduce';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        reduce: typeof reduce;
    }
}
