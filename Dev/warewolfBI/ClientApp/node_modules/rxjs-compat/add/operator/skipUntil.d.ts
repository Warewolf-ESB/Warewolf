import { skipUntil } from '../../operator/skipUntil';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        skipUntil: typeof skipUntil;
    }
}
