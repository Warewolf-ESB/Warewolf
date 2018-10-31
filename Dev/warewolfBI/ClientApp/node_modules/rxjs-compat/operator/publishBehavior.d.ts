import { ConnectableObservable, Observable } from 'rxjs';
/**
 * @param value
 * @return {ConnectableObservable<T>}
 * @method publishBehavior
 * @owner Observable
 */
export declare function publishBehavior<T>(this: Observable<T>, value: T): ConnectableObservable<T>;
