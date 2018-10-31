import { auditTime } from '../../operator/auditTime';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        auditTime: typeof auditTime;
    }
}
