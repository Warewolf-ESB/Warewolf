import { groupBy } from '../../operator/groupBy';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        groupBy: typeof groupBy;
    }
}
