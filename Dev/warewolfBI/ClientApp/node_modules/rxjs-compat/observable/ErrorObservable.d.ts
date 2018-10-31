import { Observable, SchedulerLike } from 'rxjs';
export declare class ErrorObservable<T> extends Observable<T> {
    static create<T>(error: any, scheduler?: SchedulerLike): Observable<never>;
}
