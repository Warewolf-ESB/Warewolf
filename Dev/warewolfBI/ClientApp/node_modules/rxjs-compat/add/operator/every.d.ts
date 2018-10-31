import { every } from '../../operator/every';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        every: typeof every;
    }
}
