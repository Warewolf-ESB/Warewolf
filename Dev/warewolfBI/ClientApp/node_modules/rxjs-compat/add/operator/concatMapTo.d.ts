import { concatMapTo } from '../../operator/concatMapTo';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        concatMapTo: typeof concatMapTo;
    }
}
