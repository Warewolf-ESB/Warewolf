import { elementAt } from '../../operator/elementAt';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        elementAt: typeof elementAt;
    }
}
