import { Observable, SchedulerLike } from 'rxjs';
export declare class ArrayObservable<T> extends Observable<T> {
    static create<T>(array: T[], scheduler?: SchedulerLike): Observable<T>;
}
