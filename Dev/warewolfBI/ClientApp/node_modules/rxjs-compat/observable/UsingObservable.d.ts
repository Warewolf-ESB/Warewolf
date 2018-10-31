import { Observable, Unsubscribable, SubscribableOrPromise } from 'rxjs';
export declare class UsingObservable<T> extends Observable<T> {
    static create<T>(resourceFactory: () => Unsubscribable | void, observableFactory: (resource: Unsubscribable | void) => SubscribableOrPromise<T> | void): Observable<T>;
}
