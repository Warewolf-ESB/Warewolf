import { exhaust } from '../../operator/exhaust';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        exhaust: typeof exhaust;
    }
}
