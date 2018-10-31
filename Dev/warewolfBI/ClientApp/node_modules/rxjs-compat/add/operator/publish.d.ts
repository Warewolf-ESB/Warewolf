import { publish } from '../../operator/publish';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        publish: typeof publish;
    }
}
