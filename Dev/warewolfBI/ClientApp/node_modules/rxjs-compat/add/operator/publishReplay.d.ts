import { publishReplay } from '../../operator/publishReplay';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        publishReplay: typeof publishReplay;
    }
}
