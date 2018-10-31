import { AjaxCreationMethod } from 'rxjs/internal-compatibility';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let ajax: AjaxCreationMethod;
    }
}
