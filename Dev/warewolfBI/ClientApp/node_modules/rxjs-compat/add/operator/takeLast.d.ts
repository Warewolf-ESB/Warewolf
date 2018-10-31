import { takeLast } from '../../operator/takeLast';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        takeLast: typeof takeLast;
    }
}
