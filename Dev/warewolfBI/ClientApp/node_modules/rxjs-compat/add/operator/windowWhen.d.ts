import { windowWhen } from '../../operator/windowWhen';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        windowWhen: typeof windowWhen;
    }
}
