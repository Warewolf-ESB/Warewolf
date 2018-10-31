import { Observable } from 'rxjs';
export declare function first<T, D = T>(this: Observable<T>, predicate?: null, defaultValue?: D): Observable<T | D>;
export declare function first<T, S extends T>(this: Observable<T>, predicate: (value: T, index: number, source: Observable<T>) => value is S, defaultValue?: S): Observable<S>;
export declare function first<T, D = T>(this: Observable<T>, predicate: (value: T, index: number, source: Observable<T>) => boolean, defaultValue?: D): Observable<T | D>;
