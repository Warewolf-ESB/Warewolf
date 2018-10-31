import { Observable, throwError as staticThrowError } from 'rxjs';
Observable.throw = staticThrowError;
Observable.throwError = staticThrowError;
//# sourceMappingURL=throw.js.map