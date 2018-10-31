import { mergeAll } from '../../operator/mergeAll';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        mergeAll: typeof mergeAll;
    }
}
