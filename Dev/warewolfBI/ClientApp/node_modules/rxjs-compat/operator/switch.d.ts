import { Observable, ObservableInput } from 'rxjs';
export declare function _switch<T>(this: Observable<ObservableInput<T>>): Observable<T>;
export declare function _switch<T, R>(this: Observable<T>): Observable<R>;
