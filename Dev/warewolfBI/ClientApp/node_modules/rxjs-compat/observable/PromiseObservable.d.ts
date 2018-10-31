import { Observable, SchedulerLike } from 'rxjs';
export declare class PromiseObservable<T> extends Observable<T> {
    static create<T>(promise: PromiseLike<T>, scheduler?: SchedulerLike): Observable<T>;
}
