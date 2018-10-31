import { fromEvent as staticFromEvent } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let fromEvent: typeof staticFromEvent;
    }
}
