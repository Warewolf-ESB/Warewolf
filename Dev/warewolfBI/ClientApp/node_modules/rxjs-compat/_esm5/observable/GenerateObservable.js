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
import { Observable, generate } from 'rxjs';
var GenerateObservable = (function (_super) {
    __extends(GenerateObservable, _super);
    function GenerateObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    GenerateObservable.create = function (initialStateOrOptions, condition, iterate, resultSelectorOrObservable, scheduler) {
        return generate(initialStateOrOptions, condition, iterate, resultSelectorOrObservable, scheduler);
    };
    return GenerateObservable;
}(Observable));
export { GenerateObservable };
//# sourceMappingURL=GenerateObservable.js.map