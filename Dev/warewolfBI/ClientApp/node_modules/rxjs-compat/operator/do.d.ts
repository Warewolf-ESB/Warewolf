import { Observable, PartialObserver } from 'rxjs';
export declare function _do<T>(this: Observable<T>, next: (x: T) => void, error?: (e: any) => void, complete?: () => void): Observable<T>;
export declare function _do<T>(this: Observable<T>, observer: PartialObserver<T>): Observable<T>;
