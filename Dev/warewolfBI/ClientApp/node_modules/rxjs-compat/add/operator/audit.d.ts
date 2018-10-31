import { audit } from '../../operator/audit';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        audit: typeof audit;
    }
}
