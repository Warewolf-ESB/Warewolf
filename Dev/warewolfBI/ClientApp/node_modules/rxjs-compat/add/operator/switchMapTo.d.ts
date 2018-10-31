import { switchMapTo } from '../../operator/switchMapTo';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        switchMapTo: typeof switchMapTo;
    }
}
