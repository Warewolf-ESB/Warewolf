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
import { Observable, iif } from 'rxjs';
var IfObservable = (function (_super) {
    __extends(IfObservable, _super);
    function IfObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    IfObservable.create = function (condition, thenSource, elseSource) {
        return iif(condition, thenSource, elseSource);
    };
    return IfObservable;
}(Observable));
export { IfObservable };
//# sourceMappingURL=IfObservable.js.map