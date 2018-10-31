import { _catch } from '../../operator/catch';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        catch: typeof _catch;
        _catch: typeof _catch;
    }
}
