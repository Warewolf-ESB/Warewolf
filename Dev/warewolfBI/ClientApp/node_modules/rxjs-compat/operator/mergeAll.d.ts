import { Observable, ObservableInput } from 'rxjs';
export declare function mergeAll<T>(this: Observable<ObservableInput<T>>, concurrent?: number): Observable<T>;
export declare function mergeAll<T, R>(this: Observable<T>, concurrent?: number): Observable<R>;
