import { Observable } from 'rxjs';
export declare class FromEventPatternObservable<T> extends Observable<T> {
    static create<T>(addHandler: (handler: Function) => any, removeHandler?: (handler: Function, signal?: any) => void, selector?: (...args: Array<any>) => T): Observable<T>;
}
