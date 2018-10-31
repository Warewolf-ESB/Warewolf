import { Observable, ObservableInput } from 'rxjs';
export declare function concatAll<T>(this: Observable<ObservableInput<T>>): Observable<T>;
export declare function concatAll<T, R>(this: Observable<T>): Observable<R>;
