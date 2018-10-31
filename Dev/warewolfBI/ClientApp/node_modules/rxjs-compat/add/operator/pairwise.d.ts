import { pairwise } from '../../operator/pairwise';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        pairwise: typeof pairwise;
    }
}
