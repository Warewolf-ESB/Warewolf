import { Observable, SchedulerLike } from 'rxjs';
export declare function bufferTime<T>(this: Observable<T>, bufferTimeSpan: number, scheduler?: SchedulerLike): Observable<T[]>;
export declare function bufferTime<T>(this: Observable<T>, bufferTimeSpan: number, bufferCreationInterval: number | null | undefined, scheduler?: SchedulerLike): Observable<T[]>;
export declare function bufferTime<T>(this: Observable<T>, bufferTimeSpan: number, bufferCreationInterval: number | null | undefined, maxBufferSize: number, scheduler?: SchedulerLike): Observable<T[]>;
