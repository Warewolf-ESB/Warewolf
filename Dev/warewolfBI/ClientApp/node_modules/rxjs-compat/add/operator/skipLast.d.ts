import { skipLast } from '../../operator/skipLast';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        skipLast: typeof skipLast;
    }
}
