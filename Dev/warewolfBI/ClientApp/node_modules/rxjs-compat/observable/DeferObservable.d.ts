import { Observable, SubscribableOrPromise } from 'rxjs';
export declare class DeferObservable<T> extends Observable<T> {
    static create<T>(observableFactory: () => SubscribableOrPromise<T> | void): Observable<T>;
}
