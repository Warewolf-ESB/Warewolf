import { Observable, SchedulerLike } from 'rxjs';
export declare class EmptyObservable<T> extends Observable<T> {
    static create<T>(scheduler?: SchedulerLike): Observable<T>;
}
