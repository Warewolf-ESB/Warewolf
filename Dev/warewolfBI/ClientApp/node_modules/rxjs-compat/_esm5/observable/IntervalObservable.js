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
import { Observable, asyncScheduler, interval } from 'rxjs';
var IntervalObservable = (function (_super) {
    __extends(IntervalObservable, _super);
    function IntervalObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    IntervalObservable.create = function (period, scheduler) {
        if (period === void 0) { period = 0; }
        if (scheduler === void 0) { scheduler = asyncScheduler; }
        return interval(period, scheduler);
    };
    return IntervalObservable;
}(Observable));
export { IntervalObservable };
//# sourceMappingURL=IntervalObservable.js.map