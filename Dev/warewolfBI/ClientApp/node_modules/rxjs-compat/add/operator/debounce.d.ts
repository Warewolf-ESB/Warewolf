import { debounce } from '../../operator/debounce';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        debounce: typeof debounce;
    }
}
