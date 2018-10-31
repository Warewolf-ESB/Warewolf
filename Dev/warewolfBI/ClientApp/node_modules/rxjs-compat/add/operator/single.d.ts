import { single } from '../../operator/single';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        single: typeof single;
    }
}
