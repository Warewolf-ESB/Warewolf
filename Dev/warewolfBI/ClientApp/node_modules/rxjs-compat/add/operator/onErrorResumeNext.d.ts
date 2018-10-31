import { onErrorResumeNext } from '../../operator/onErrorResumeNext';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        onErrorResumeNext: typeof onErrorResumeNext;
    }
}
