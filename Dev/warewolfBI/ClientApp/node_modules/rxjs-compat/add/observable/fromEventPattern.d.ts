import { fromEventPattern as staticFromEventPattern } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let fromEventPattern: typeof staticFromEventPattern;
    }
}
