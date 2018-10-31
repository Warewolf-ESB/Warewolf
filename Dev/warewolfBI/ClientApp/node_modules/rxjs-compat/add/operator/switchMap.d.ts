import { switchMap } from '../../operator/switchMap';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        switchMap: typeof switchMap;
    }
}
