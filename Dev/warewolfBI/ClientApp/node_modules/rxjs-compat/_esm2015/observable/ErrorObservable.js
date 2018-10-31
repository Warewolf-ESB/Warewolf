import { Observable, throwError } from 'rxjs';
export class ErrorObservable extends Observable {
    static create(error, scheduler) {
        return throwError(error, scheduler);
    }
}
//# sourceMappingURL=ErrorObservable.js.map