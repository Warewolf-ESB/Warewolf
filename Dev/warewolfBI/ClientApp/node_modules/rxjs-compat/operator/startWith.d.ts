import { Observable, SchedulerLike } from 'rxjs';
export declare function startWith<T>(this: Observable<T>, scheduler?: SchedulerLike): Observable<T>;
export declare function startWith<T, D = T>(this: Observable<T>, v1: D, scheduler?: SchedulerLike): Observable<T | D>;
export declare function startWith<T, D = T, E = T>(this: Observable<T>, v1: D, v2: E, scheduler?: SchedulerLike): Observable<T | D | E>;
export declare function startWith<T, D = T, E = T, F = T>(this: Observable<T>, v1: D, v2: E, v3: F, scheduler?: SchedulerLike): Observable<T | D | E | F>;
export declare function startWith<T, D = T, E = T, F = T, G = T>(this: Observable<T>, v1: D, v2: E, v3: F, v4: G, scheduler?: SchedulerLike): Observable<T | D | E | F | G>;
export declare function startWith<T, D = T, E = T, F = T, G = T, H = T>(this: Observable<T>, v1: D, v2: E, v3: F, v4: G, v5: H, scheduler?: SchedulerLike): Observable<T | D | E | F | G | H>;
export declare function startWith<T, D = T, E = T, F = T, G = T, H = T, I = T>(this: Observable<T>, v1: D, v2: E, v3: F, v4: G, v5: H, v6: I, scheduler?: SchedulerLike): Observable<T | D | E | F | G | H | I>;
export declare function startWith<T, D = T>(this: Observable<T>, ...array: Array<D | SchedulerLike>): Observable<T | D>;
