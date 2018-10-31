import { Observable, SchedulerLike } from 'rxjs';
import { TimeInterval } from 'rxjs/internal-compatibility';
/**
 * @param scheduler
 * @return {Observable<TimeInterval<any>>|WebSocketSubject<T>|Observable<T>}
 * @method timeInterval
 * @owner Observable
 */
export declare function timeInterval<T>(this: Observable<T>, scheduler?: SchedulerLike): Observable<TimeInterval<T>>;
