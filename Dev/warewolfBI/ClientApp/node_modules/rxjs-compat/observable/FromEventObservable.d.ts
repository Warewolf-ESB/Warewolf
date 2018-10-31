import { Observable } from 'rxjs';
import { EventTargetLike } from 'rxjs/internal-compatibility';
export declare class FromEventObservable<T> extends Observable<T> {
    static create<T>(target: EventTargetLike<T>, eventName: string): Observable<T>;
    static create<T>(target: EventTargetLike<T>, eventName: string, selector: ((...args: any[]) => T)): Observable<T>;
    static create<T>(target: EventTargetLike<T>, eventName: string, options: EventListenerOptions): Observable<T>;
    static create<T>(target: EventTargetLike<T>, eventName: string, options: EventListenerOptions, selector: ((...args: any[]) => T)): Observable<T>;
}
