import { dematerialize } from '../../operator/dematerialize';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        dematerialize: typeof dematerialize;
    }
}
