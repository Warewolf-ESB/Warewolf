import { mapTo } from '../../operator/mapTo';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        mapTo: typeof mapTo;
    }
}
