import { Observable } from 'rxjs';
export declare function find<T, S extends T>(this: Observable<T>, predicate: (value: T, index: number) => value is S, thisArg?: any): Observable<S | undefined>;
export declare function find<T>(this: Observable<T>, predicate: (value: T, index: number) => boolean, thisArg?: any): Observable<T | undefined>;
