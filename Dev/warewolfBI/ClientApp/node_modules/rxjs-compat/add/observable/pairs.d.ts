import { pairs as staticPairs } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let pairs: typeof staticPairs;
    }
}
