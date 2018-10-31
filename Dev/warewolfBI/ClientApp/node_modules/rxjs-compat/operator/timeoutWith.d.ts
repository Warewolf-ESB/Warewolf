import { Observable, ObservableInput, SchedulerLike } from 'rxjs';
export declare function timeoutWith<T>(this: Observable<T>, due: number | Date, withObservable: ObservableInput<T>, scheduler?: SchedulerLike): Observable<T>;
export declare function timeoutWith<T, R>(this: Observable<T>, due: number | Date, withObservable: ObservableInput<R>, scheduler?: SchedulerLike): Observable<T | R>;
