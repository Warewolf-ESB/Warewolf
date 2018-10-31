import { merge } from '../../operator/merge';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        merge: typeof merge;
    }
}
