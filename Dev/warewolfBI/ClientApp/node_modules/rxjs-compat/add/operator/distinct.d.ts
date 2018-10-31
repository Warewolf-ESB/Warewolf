import { distinct } from '../../operator/distinct';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        distinct: typeof distinct;
    }
}
