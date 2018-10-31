import { concat } from '../../operator/concat';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        concat: typeof concat;
    }
}
