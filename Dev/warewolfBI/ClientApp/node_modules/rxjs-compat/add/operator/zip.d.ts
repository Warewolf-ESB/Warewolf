import { zipProto } from '../../operator/zip';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        zip: typeof zipProto;
    }
}
