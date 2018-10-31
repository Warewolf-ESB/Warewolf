import { find } from '../../operator/find';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        find: typeof find;
    }
}
