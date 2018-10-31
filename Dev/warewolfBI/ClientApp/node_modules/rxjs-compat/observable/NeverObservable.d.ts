import { Observable } from 'rxjs';
export declare class NeverObservable<T> extends Observable<T> {
    static create<T>(): Observable<never>;
}
