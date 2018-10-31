import { materialize } from '../../operator/materialize';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        materialize: typeof materialize;
    }
}
