import { exhaustMap } from '../../operator/exhaustMap';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        exhaustMap: typeof exhaustMap;
    }
}
