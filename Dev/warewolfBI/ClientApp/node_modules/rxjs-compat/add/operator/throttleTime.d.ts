import { throttleTime } from '../../operator/throttleTime';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        throttleTime: typeof throttleTime;
    }
}
