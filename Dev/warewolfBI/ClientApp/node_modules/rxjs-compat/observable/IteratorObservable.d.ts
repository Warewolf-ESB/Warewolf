import { Observable, SchedulerLike } from 'rxjs';
export declare class IteratorObservable<T> extends Observable<T> {
    static create<T>(iterator: any, scheduler?: SchedulerLike): Observable<T>;
}
