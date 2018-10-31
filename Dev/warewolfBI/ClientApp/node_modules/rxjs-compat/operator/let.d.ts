import { Observable } from 'rxjs';
/**
 * @param func
 * @return {Observable<R>}
 * @method let
 * @owner Observable
 */
export declare function letProto<T, R>(this: Observable<T>, func: (selector: Observable<T>) => Observable<R>): Observable<R>;
