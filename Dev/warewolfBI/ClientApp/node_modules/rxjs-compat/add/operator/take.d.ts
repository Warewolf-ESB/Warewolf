import { take } from '../../operator/take';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        take: typeof take;
    }
}
