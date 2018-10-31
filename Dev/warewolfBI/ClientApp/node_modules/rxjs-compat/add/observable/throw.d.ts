import { throwError as staticThrowError } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let throwError: typeof staticThrowError;
    }
}
