import { race as staticRace } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let race: typeof staticRace;
    }
}
