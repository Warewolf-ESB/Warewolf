import { bufferCount } from '../../operator/bufferCount';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        bufferCount: typeof bufferCount;
    }
}
