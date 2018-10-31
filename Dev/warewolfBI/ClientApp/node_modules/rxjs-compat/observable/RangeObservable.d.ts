import { Observable, SchedulerLike } from 'rxjs';
export declare class RangeObservable<T> extends Observable<T> {
    static create(start?: number, count?: number, scheduler?: SchedulerLike): Observable<number>;
}
