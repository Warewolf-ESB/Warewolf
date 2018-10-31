import { zipAll } from '../../operator/zipAll';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        zipAll: typeof zipAll;
    }
}
