import { concatMap } from '../../operator/concatMap';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        concatMap: typeof concatMap;
    }
}
