import { defer as staticDefer } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let defer: typeof staticDefer;
    }
}
