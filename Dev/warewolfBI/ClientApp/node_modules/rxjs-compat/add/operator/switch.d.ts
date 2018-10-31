import { _switch } from '../../operator/switch';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        switch: typeof _switch;
        _switch: typeof _switch;
    }
}
