import { Observable, SchedulerLike } from 'rxjs';
import { ConditionFunc, IterateFunc, ResultFunc, GenerateBaseOptions, GenerateOptions } from 'rxjs/internal-compatibility';
export declare class GenerateObservable<T> extends Observable<T> {
    static create<T, S>(initialState: S, condition: ConditionFunc<S>, iterate: IterateFunc<S>, resultSelector: ResultFunc<S, T>, scheduler?: SchedulerLike): Observable<T>;
    static create<S>(initialState: S, condition: ConditionFunc<S>, iterate: IterateFunc<S>, scheduler?: SchedulerLike): Observable<S>;
    static create<S>(options: GenerateBaseOptions<S>): Observable<S>;
    static create<T, S>(options: GenerateOptions<T, S>): Observable<T>;
}
