import { expand } from '../../operator/expand';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        expand: typeof expand;
    }
}
