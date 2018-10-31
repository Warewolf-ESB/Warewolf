import { bufferWhen } from '../../operator/bufferWhen';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        bufferWhen: typeof bufferWhen;
    }
}
