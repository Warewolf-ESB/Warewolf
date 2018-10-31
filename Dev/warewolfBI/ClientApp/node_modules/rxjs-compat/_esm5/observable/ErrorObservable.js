var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
import { Observable, throwError } from 'rxjs';
var ErrorObservable = (function (_super) {
    __extends(ErrorObservable, _super);
    function ErrorObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    ErrorObservable.create = function (error, scheduler) {
        return throwError(error, scheduler);
    };
    return ErrorObservable;
}(Observable));
export { ErrorObservable };
//# sourceMappingURL=ErrorObservable.js.map