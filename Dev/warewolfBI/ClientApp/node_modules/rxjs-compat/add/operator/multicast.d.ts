import { multicast } from '../../operator/multicast';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        multicast: typeof multicast;
    }
}
