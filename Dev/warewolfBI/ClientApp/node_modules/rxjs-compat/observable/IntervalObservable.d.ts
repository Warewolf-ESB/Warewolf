import { Observable, SchedulerLike } from 'rxjs';
export declare class IntervalObservable<T> extends Observable<T> {
    static create(period?: number, scheduler?: SchedulerLike): Observable<number>;
}
