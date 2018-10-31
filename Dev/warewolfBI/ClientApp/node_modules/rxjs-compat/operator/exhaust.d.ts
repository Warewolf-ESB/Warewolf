import { Observable, ObservableInput } from 'rxjs';
export declare function exhaust<T>(this: Observable<ObservableInput<T>>): Observable<T>;
export declare function exhaust<T, R>(this: Observable<T>): Observable<R>;
