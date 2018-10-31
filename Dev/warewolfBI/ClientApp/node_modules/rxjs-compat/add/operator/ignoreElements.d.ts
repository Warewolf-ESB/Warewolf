import { ignoreElements } from '../../operator/ignoreElements';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        ignoreElements: typeof ignoreElements;
    }
}
