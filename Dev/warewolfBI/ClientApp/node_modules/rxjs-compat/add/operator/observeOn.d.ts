import { observeOn } from '../../operator/observeOn';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        observeOn: typeof observeOn;
    }
}
