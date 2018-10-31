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
import { Observable, using } from 'rxjs';
var UsingObservable = (function (_super) {
    __extends(UsingObservable, _super);
    function UsingObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    UsingObservable.create = function (resourceFactory, observableFactory) {
        return using(resourceFactory, observableFactory);
    };
    return UsingObservable;
}(Observable));
export { UsingObservable };
//# sourceMappingURL=UsingObservable.js.map