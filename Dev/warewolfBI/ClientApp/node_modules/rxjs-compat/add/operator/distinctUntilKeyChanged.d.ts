import { distinctUntilKeyChanged } from '../../operator/distinctUntilKeyChanged';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        distinctUntilKeyChanged: typeof distinctUntilKeyChanged;
    }
}
