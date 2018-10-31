import { findIndex } from '../../operator/findIndex';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        findIndex: typeof findIndex;
    }
}
