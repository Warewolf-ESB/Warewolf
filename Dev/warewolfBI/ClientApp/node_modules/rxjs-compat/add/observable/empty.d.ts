import { empty as staticEmpty } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let empty: typeof staticEmpty;
    }
}
