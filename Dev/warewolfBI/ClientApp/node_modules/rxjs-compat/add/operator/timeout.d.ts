import { timeout } from '../../operator/timeout';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        timeout: typeof timeout;
    }
}
