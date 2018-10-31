import { timestamp } from '../../operator/timestamp';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        timestamp: typeof timestamp;
    }
}
