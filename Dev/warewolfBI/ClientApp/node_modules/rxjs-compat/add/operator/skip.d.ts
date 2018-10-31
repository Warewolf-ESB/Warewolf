import { skip } from '../../operator/skip';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        skip: typeof skip;
    }
}
