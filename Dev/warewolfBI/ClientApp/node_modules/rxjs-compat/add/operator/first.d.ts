import { first } from '../../operator/first';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        first: typeof first;
    }
}
