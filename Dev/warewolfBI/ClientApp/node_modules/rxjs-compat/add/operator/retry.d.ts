import { retry } from '../../operator/retry';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        retry: typeof retry;
    }
}
