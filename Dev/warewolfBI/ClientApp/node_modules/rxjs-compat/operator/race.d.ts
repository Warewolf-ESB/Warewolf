import { Observable } from 'rxjs';
export declare function race<T>(this: Observable<T>, observables: Array<Observable<T>>): Observable<T>;
export declare function race<T, R>(this: Observable<T>, observables: Array<Observable<T>>): Observable<R>;
export declare function race<T>(this: Observable<T>, ...observables: Array<Observable<T> | Array<Observable<T>>>): Observable<T>;
export declare function race<T, R>(this: Observable<T>, ...observables: Array<Observable<any> | Array<Observable<any>>>): Observable<R>;
