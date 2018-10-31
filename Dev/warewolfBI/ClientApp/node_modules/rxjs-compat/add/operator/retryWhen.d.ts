import { retryWhen } from '../../operator/retryWhen';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        retryWhen: typeof retryWhen;
    }
}
