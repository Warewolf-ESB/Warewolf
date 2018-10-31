import { buffer } from '../../operator/buffer';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        buffer: typeof buffer;
    }
}
