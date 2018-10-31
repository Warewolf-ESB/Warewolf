import { Observable } from 'rxjs';
export declare function staticNever(): Observable<never>;
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let never: typeof staticNever;
    }
}
