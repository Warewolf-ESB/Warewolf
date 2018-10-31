import { of as staticOf } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let of: typeof staticOf;
    }
}
