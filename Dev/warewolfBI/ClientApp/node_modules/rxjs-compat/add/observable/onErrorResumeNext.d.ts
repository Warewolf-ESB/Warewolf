import { onErrorResumeNext as staticOnErrorResumeNext } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let onErrorResumeNext: typeof staticOnErrorResumeNext;
    }
}
