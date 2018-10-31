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
import { Observable, empty } from 'rxjs';
var EmptyObservable = (function (_super) {
    __extends(EmptyObservable, _super);
    function EmptyObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    EmptyObservable.create = function (scheduler) {
        return empty(scheduler);
    };
    return EmptyObservable;
}(Observable));
export { EmptyObservable };
//# sourceMappingURL=EmptyObservable.js.map