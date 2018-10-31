import { timer as staticTimer } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let timer: typeof staticTimer;
    }
}
