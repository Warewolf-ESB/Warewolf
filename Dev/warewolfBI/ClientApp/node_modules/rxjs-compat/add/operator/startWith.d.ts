import { startWith } from '../../operator/startWith';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        startWith: typeof startWith;
    }
}
