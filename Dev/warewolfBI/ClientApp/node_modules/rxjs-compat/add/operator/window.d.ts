import { window } from '../../operator/window';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        window: typeof window;
    }
}
