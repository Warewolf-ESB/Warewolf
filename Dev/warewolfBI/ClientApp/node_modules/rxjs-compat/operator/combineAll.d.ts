import { Observable, ObservableInput } from 'rxjs';
export declare function combineAll<T>(this: Observable<ObservableInput<T>>): Observable<T[]>;
export declare function combineAll<T, R>(this: Observable<T>): Observable<R[]>;
export declare function combineAll<T, R>(this: Observable<ObservableInput<T>>, project: (...values: T[]) => R): Observable<R>;
export declare function combineAll<T, R>(this: Observable<T>, project: (...values: T[]) => R): Observable<R>;
export declare function combineAll<R>(this: Observable<any>, project: (...values: any[]) => R): Observable<R>;
