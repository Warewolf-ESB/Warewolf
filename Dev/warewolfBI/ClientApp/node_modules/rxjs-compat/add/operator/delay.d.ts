import { delay } from '../../operator/delay';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        delay: typeof delay;
    }
}
