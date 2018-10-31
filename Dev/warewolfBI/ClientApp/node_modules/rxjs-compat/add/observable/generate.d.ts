import { generate as staticGenerate } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let generate: typeof staticGenerate;
    }
}
