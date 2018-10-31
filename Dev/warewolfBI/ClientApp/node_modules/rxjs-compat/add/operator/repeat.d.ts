import { repeat } from '../../operator/repeat';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        repeat: typeof repeat;
    }
}
