import { publishBehavior } from '../../operator/publishBehavior';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        publishBehavior: typeof publishBehavior;
    }
}
