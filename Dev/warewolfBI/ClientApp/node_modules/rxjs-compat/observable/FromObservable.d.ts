import { Observable, ObservableInput, SchedulerLike } from 'rxjs';
export declare class FromObservable<T> extends Observable<T> {
    static create<T>(ish: ObservableInput<T>, scheduler?: SchedulerLike): Observable<T>;
    static create<T, R>(ish: ArrayLike<T>, scheduler?: SchedulerLike): Observable<R>;
}
