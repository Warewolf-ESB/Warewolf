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
import { Observable, from } from 'rxjs';
var FromObservable = (function (_super) {
    __extends(FromObservable, _super);
    function FromObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    FromObservable.create = function (ish, scheduler) {
        return from(ish, scheduler);
    };
    return FromObservable;
}(Observable));
export { FromObservable };
//# sourceMappingURL=FromObservable.js.map