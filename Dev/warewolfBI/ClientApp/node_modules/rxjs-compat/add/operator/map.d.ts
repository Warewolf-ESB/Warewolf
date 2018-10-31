import { map } from '../../operator/map';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        map: typeof map;
    }
}
