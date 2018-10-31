import { sequenceEqual } from '../../operator/sequenceEqual';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        sequenceEqual: typeof sequenceEqual;
    }
}
