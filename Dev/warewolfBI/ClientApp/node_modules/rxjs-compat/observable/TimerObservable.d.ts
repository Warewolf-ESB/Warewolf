import { Observable, SchedulerLike } from 'rxjs';
export declare class TimerObservable<T> extends Observable<T> {
    static create(initialDelay?: number | Date, period?: number | SchedulerLike, scheduler?: SchedulerLike): Observable<number>;
}
