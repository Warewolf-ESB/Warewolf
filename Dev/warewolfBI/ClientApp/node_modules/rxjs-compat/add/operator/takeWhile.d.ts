import { takeWhile } from '../../operator/takeWhile';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        takeWhile: typeof takeWhile;
    }
}
