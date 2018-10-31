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
import { Observable, defer } from 'rxjs';
var DeferObservable = (function (_super) {
    __extends(DeferObservable, _super);
    function DeferObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DeferObservable.create = function (observableFactory) {
        return defer(observableFactory);
    };
    return DeferObservable;
}(Observable));
export { DeferObservable };
//# sourceMappingURL=DeferObservable.js.map