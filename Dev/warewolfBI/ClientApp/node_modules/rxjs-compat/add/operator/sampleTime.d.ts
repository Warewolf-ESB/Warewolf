import { sampleTime } from '../../operator/sampleTime';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        sampleTime: typeof sampleTime;
    }
}
