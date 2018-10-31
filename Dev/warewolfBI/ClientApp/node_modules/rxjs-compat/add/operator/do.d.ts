import { _do } from '../../operator/do';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        do: typeof _do;
        _do: typeof _do;
    }
}
