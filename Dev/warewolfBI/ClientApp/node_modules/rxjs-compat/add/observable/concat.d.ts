import { concat as concatStatic } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let concat: typeof concatStatic;
    }
}
