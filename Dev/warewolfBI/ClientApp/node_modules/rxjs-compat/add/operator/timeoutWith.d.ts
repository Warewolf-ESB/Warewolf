import { timeoutWith } from '../../operator/timeoutWith';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        timeoutWith: typeof timeoutWith;
    }
}
