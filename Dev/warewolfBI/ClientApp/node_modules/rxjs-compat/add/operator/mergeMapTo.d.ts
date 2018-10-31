import { mergeMapTo } from '../../operator/mergeMapTo';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        flatMapTo: typeof mergeMapTo;
        mergeMapTo: typeof mergeMapTo;
    }
}
