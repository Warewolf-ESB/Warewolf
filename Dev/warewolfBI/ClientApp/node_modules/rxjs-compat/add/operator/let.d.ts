import { letProto } from '../../operator/let';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        let: typeof letProto;
        letBind: typeof letProto;
    }
}
