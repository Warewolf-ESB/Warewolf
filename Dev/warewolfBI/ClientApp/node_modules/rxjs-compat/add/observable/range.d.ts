import { range as staticRange } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let range: typeof staticRange;
    }
}
