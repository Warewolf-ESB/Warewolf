import { Observable, SchedulerLike } from 'rxjs';
import { Timestamp } from 'rxjs/internal-compatibility';
/**
 * @param scheduler
 * @return {Observable<Timestamp<any>>|WebSocketSubject<T>|Observable<T>}
 * @method timestamp
 * @owner Observable
 */
export declare function timestamp<T>(this: Observable<T>, scheduler?: SchedulerLike): Observable<Timestamp<T>>;
