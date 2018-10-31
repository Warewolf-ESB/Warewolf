import { partition } from '../../operator/partition';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        partition: typeof partition;
    }
}
