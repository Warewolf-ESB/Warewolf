import { bufferTime } from '../../operator/bufferTime';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        bufferTime: typeof bufferTime;
    }
}
