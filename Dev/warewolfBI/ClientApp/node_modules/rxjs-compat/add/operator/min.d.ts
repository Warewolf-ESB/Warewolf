import { min } from '../../operator/min';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        min: typeof min;
    }
}
