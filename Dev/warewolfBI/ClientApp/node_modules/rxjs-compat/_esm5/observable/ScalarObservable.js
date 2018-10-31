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
import { Observable, of } from 'rxjs';
var ScalarObservable = (function (_super) {
    __extends(ScalarObservable, _super);
    function ScalarObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    ScalarObservable.create = function (value, scheduler) {
        return arguments.length > 1 ? of(value, scheduler) : of(value);
    };
    return ScalarObservable;
}(Observable));
export { ScalarObservable };
//# sourceMappingURL=ScalarObservable.js.map