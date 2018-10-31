import { last } from '../../operator/last';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        last: typeof last;
    }
}
