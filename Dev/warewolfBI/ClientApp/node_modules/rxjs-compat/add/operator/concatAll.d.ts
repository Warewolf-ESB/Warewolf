import { concatAll } from '../../operator/concatAll';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        concatAll: typeof concatAll;
    }
}
