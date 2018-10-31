import { max } from '../../operator/max';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        max: typeof max;
    }
}
