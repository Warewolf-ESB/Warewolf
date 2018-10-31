import { mergeScan } from '../../operator/mergeScan';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        mergeScan: typeof mergeScan;
    }
}
