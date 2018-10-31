import { zip as zipStatic } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let zip: typeof zipStatic;
    }
}
