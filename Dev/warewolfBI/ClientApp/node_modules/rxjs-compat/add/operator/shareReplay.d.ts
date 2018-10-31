import { shareReplay } from '../../operator/shareReplay';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        shareReplay: typeof shareReplay;
    }
}
