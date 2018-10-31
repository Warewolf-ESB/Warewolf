import { isEmpty } from '../../operator/isEmpty';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        isEmpty: typeof isEmpty;
    }
}
