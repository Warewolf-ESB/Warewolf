import { Observable, SchedulerLike } from 'rxjs';
export declare class ArrayLikeObservable<T> extends Observable<T> {
    static create<T>(arrayLike: ArrayLike<T>, scheduler?: SchedulerLike): Observable<T>;
}
