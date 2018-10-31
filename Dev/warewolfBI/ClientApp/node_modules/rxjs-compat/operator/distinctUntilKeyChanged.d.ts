import { Observable } from 'rxjs';
export declare function distinctUntilKeyChanged<T>(this: Observable<T>, key: keyof T): Observable<T>;
export declare function distinctUntilKeyChanged<T, K extends keyof T>(this: Observable<T>, key: K, compare: (x: T[K], y: T[K]) => boolean): Observable<T>;
