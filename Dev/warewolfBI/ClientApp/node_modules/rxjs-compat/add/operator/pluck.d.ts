import { pluck } from '../../operator/pluck';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        pluck: typeof pluck;
    }
}
