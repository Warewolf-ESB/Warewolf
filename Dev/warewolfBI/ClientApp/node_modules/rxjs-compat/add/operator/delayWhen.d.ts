import { delayWhen } from '../../operator/delayWhen';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        delayWhen: typeof delayWhen;
    }
}
