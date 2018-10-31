import { windowCount } from '../../operator/windowCount';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        windowCount: typeof windowCount;
    }
}
