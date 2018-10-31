import { share } from '../../operator/share';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        share: typeof share;
    }
}
