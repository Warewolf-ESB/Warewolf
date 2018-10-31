import { Observable, SchedulerLike } from 'rxjs';
/**
 * @method shareReplay
 * @owner Observable
 */
export declare function shareReplay<T>(this: Observable<T>, bufferSize?: number, windowTime?: number, scheduler?: SchedulerLike): Observable<T>;
