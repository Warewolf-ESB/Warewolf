import { ConnectableObservable, Observable } from 'rxjs';
export declare function publish<T>(this: Observable<T>): ConnectableObservable<T>;
export declare function publish<T, R>(this: Observable<T>, selector: (source: Observable<T>) => Observable<R>): Observable<R>;
export declare function publish<T>(this: Observable<T>, selector: (source: Observable<T>) => Observable<T>): Observable<T>;
export declare type selector<T> = (source: Observable<T>) => Observable<T>;
