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
import { Observable, range } from 'rxjs';
var RangeObservable = (function (_super) {
    __extends(RangeObservable, _super);
    function RangeObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    RangeObservable.create = function (start, count, scheduler) {
        if (start === void 0) { start = 0; }
        if (count === void 0) { count = 0; }
        return range(start, count, scheduler);
    };
    return RangeObservable;
}(Observable));
export { RangeObservable };
//# sourceMappingURL=RangeObservable.js.map