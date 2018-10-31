import { interval as staticInterval } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let interval: typeof staticInterval;
    }
}
