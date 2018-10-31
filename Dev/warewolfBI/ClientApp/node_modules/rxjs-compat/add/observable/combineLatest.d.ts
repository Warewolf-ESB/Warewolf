import { combineLatest as combineLatestStatic } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let combineLatest: typeof combineLatestStatic;
    }
}
