import { skipWhile } from '../../operator/skipWhile';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        skipWhile: typeof skipWhile;
    }
}
