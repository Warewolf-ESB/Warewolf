import { timeInterval } from '../../operator/timeInterval';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        timeInterval: typeof timeInterval;
    }
}
