import { Observable, SchedulerLike } from 'rxjs';
export declare class ScalarObservable<T> extends Observable<T> {
    static create<T>(value: T, scheduler?: SchedulerLike): Observable<T>;
}
