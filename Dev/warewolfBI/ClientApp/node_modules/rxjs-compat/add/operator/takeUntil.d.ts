import { takeUntil } from '../../operator/takeUntil';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        takeUntil: typeof takeUntil;
    }
}
