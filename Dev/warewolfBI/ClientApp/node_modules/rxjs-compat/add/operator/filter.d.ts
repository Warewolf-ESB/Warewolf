import { filter } from '../../operator/filter';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        filter: typeof filter;
    }
}
