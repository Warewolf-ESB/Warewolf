import { bindCallback as staticBindCallback } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let bindCallback: typeof staticBindCallback;
    }
}
