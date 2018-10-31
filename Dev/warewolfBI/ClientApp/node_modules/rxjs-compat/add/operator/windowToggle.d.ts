import { windowToggle } from '../../operator/windowToggle';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        windowToggle: typeof windowToggle;
    }
}
