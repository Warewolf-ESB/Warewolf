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
var IteratorObservable = (function (_super) {
    __extends(IteratorObservable, _super);
    function IteratorObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    IteratorObservable.create = function (iterator, scheduler) {
        return from(iterator, scheduler);
    };
    return IteratorObservable;
}(Observable));
export { IteratorObservable };
//# sourceMappingURL=IteratorObservable.js.map