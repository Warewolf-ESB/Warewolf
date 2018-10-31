import { forkJoin as staticForkJoin } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let forkJoin: typeof staticForkJoin;
    }
}
