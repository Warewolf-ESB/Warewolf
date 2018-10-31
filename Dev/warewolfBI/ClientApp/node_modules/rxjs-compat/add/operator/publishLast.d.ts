import { publishLast } from '../../operator/publishLast';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        publishLast: typeof publishLast;
    }
}
