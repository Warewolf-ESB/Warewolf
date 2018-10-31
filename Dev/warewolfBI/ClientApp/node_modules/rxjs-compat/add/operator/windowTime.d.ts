import { windowTime } from '../../operator/windowTime';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        windowTime: typeof windowTime;
    }
}
