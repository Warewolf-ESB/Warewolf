import { repeatWhen } from '../../operator/repeatWhen';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        repeatWhen: typeof repeatWhen;
    }
}
