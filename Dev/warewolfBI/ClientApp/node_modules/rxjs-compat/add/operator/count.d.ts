import { count } from '../../operator/count';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        count: typeof count;
    }
}
