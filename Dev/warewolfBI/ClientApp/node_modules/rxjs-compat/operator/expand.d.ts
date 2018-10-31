import { Observable, ObservableInput, SchedulerLike } from 'rxjs';
export declare function expand<T, R>(this: Observable<T>, project: (value: T, index: number) => ObservableInput<R>, concurrent?: number, scheduler?: SchedulerLike): Observable<R>;
export declare function expand<T>(this: Observable<T>, project: (value: T, index: number) => ObservableInput<T>, concurrent?: number, scheduler?: SchedulerLike): Observable<T>;
