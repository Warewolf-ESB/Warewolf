import { throttle } from '../../operator/throttle';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        throttle: typeof throttle;
    }
}
