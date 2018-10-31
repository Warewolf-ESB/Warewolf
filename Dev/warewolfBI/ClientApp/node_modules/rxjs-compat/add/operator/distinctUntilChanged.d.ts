import { distinctUntilChanged } from '../../operator/distinctUntilChanged';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        distinctUntilChanged: typeof distinctUntilChanged;
    }
}
