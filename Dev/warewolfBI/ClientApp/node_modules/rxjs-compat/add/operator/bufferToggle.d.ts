import { bufferToggle } from '../../operator/bufferToggle';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        bufferToggle: typeof bufferToggle;
    }
}
