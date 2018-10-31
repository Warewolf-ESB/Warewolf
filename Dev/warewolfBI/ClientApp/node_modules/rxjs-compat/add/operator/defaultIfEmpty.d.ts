import { defaultIfEmpty } from '../../operator/defaultIfEmpty';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        defaultIfEmpty: typeof defaultIfEmpty;
    }
}
