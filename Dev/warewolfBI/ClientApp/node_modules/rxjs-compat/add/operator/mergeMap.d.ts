import { mergeMap } from '../../operator/mergeMap';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        flatMap: typeof mergeMap;
        mergeMap: typeof mergeMap;
    }
}
