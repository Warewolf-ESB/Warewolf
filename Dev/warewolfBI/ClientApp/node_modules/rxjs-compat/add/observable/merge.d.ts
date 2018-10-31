import { merge as mergeStatic } from 'rxjs';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let merge: typeof mergeStatic;
    }
}
