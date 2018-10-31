import { Observable, SchedulerLike } from 'rxjs';
export declare class PairsObservable<T> extends Observable<T> {
    static create<T>(obj: Object, scheduler?: SchedulerLike): Observable<(string | T)[]>;
}
