import { subscribeOn } from '../../operator/subscribeOn';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        subscribeOn: typeof subscribeOn;
    }
}
