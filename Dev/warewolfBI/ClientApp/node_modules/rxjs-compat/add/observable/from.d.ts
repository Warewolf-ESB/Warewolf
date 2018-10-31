import { from as staticFrom } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let from: typeof staticFrom;
    }
}
