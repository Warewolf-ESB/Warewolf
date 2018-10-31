import { fromPromise as staticFromPromise } from 'rxjs/internal-compatibility';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let fromPromise: typeof staticFromPromise;
    }
}
