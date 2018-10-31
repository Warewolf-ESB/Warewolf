import { ConnectableObservable, Observable, OperatorFunction, MonoTypeOperatorFunction, SchedulerLike } from 'rxjs';
export declare function publishReplay<T>(this: Observable<T>, bufferSize?: number, windowTime?: number, scheduler?: SchedulerLike): ConnectableObservable<T>;
export declare function publishReplay<T, R>(this: Observable<T>, bufferSize?: number, windowTime?: number, selector?: OperatorFunction<T, R>): Observable<R>;
export declare function publishReplay<T>(this: Observable<T>, bufferSize?: number, windowTime?: number, selector?: MonoTypeOperatorFunction<T>, scheduler?: SchedulerLike): Observable<T>;
