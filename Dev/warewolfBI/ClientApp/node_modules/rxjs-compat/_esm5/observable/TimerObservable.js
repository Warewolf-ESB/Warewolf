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
import { Observable, timer } from 'rxjs';
var TimerObservable = (function (_super) {
    __extends(TimerObservable, _super);
    function TimerObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    TimerObservable.create = function (initialDelay, period, scheduler) {
        if (initialDelay === void 0) { initialDelay = 0; }
        return timer(initialDelay, period, scheduler);
    };
    return TimerObservable;
}(Observable));
export { TimerObservable };
//# sourceMappingURL=TimerObservable.js.map