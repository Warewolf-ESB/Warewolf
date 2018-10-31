import { scan } from '../../operator/scan';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        scan: typeof scan;
    }
}
