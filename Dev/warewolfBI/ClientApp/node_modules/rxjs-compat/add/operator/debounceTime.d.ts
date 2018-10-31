import { debounceTime } from '../../operator/debounceTime';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        debounceTime: typeof debounceTime;
    }
}
