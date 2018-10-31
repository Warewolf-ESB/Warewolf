import { Observable, SubscribableOrPromise } from 'rxjs';
export declare class IfObservable<T> extends Observable<T> {
    static create<T, R>(condition: () => boolean | void, thenSource?: SubscribableOrPromise<T> | void, elseSource?: SubscribableOrPromise<R> | void): Observable<T | R>;
}
