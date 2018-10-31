import { sample } from '../../operator/sample';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        sample: typeof sample;
    }
}
