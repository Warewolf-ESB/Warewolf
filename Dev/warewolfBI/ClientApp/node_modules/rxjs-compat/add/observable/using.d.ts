import { using as staticUsing } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let using: typeof staticUsing;
    }
}
