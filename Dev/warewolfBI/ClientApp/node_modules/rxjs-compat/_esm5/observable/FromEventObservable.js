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
import { Observable, fromEvent } from 'rxjs';
var FromEventObservable = (function (_super) {
    __extends(FromEventObservable, _super);
    function FromEventObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    FromEventObservable.create = function (target, eventName, options, selector) {
        return fromEvent(target, eventName, options, selector);
    };
    return FromEventObservable;
}(Observable));
export { FromEventObservable };
//# sourceMappingURL=FromEventObservable.js.map