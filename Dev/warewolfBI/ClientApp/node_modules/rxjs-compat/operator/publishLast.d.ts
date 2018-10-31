import { ConnectableObservable, Observable } from 'rxjs';
/**
 * @return {ConnectableObservable<T>}
 * @method publishLast
 * @owner Observable
 */
export declare function publishLast<T>(this: Observable<T>): ConnectableObservable<T>;
