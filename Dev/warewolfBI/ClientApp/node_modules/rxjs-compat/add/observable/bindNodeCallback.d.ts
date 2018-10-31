import { bindNodeCallback as staticBindNodeCallback } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let bindNodeCallback: typeof staticBindNodeCallback;
    }
}
