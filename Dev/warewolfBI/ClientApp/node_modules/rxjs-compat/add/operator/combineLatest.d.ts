import { combineLatest } from '../../operator/combineLatest';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        combineLatest: typeof combineLatest;
    }
}
