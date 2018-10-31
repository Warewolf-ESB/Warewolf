import { withLatestFrom } from '../../operator/withLatestFrom';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        withLatestFrom: typeof withLatestFrom;
    }
}
