import { Observable, ObservableInput } from 'rxjs';
export declare function zipAll<T>(this: Observable<ObservableInput<T>>): Observable<T[]>;
export declare function zipAll<T, R>(this: Observable<T>): Observable<R[]>;
export declare function zipAll<T, R>(this: Observable<ObservableInput<T>>, project: (...values: T[]) => R): Observable<R>;
export declare function zipAll<T, R>(this: Observable<T>, project: (...values: T[]) => R): Observable<R>;
export declare function zipAll<R>(this: Observable<any>, project: (...values: any[]) => R): Observable<R>;
